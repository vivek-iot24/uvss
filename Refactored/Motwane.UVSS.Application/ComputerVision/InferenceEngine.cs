using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;

namespace Motwane.UVSS.Application.ComputerVision
{
    public class YoloDetector : IDisposable
    {
        private readonly InferenceSession _session;
        private readonly int _width;
        private readonly int _height;
        private readonly string[] _labels;

        public YoloDetector(string modelPath, string[] labels)
        {
            var opts = new SessionOptions();
            opts.AppendExecutionProvider_CPU(1);
            _session = new InferenceSession(modelPath, opts);
            _width = 320; // Default YOLOv8 input size
            _height = 320;
            _labels = labels;
        }

        public List<DetectionResult> Detect(Mat image)
        {
            // Preprocessing
            float ratio = Math.Min((float)_width / image.Width, (float)_height / image.Height);
            int newWidth = (int)(image.Width * ratio);
            int newHeight = (int)(image.Height * ratio);

            using (var resized = new Mat())
            {
                Cv2.Resize(image, resized, new Size(newWidth, newHeight));

                using (var padded = new Mat(new Size(_width, _height), MatType.CV_8UC3, Scalar.All(114)))
                {
                    resized.CopyTo(padded[new Rect(0, 0, newWidth, newHeight)]);

                    var tensor = MatToTensor(padded);

                    var inputs = new List<NamedOnnxValue>
                    {
                        NamedOnnxValue.CreateFromTensor("images", tensor)
                    };

                    using (var results = _session.Run(inputs))
                    {
                        var output = results.First().AsTensor<float>();
                       
                        return Postprocess(output, image.Width, image.Height, ratio);
                    }
                 
                }
            }

            
        }

        private Tensor<float> MatToTensor(Mat img)
        {
            var tensor = new DenseTensor<float>(new[] { 1, 3, _height, _width });
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    var color = img.At<Vec3b>(y, x);
                    tensor[0, 0, y, x] = color.Item2 / 255.0f; // R
                    tensor[0, 1, y, x] = color.Item1 / 255.0f; // G
                    tensor[0, 2, y, x] = color.Item0 / 255.0f; // B
                }
            }
            return tensor;
        }

        private List<DetectionResult> Postprocess(Tensor<float> output, int origWidth, int origHeight, float ratio)
        {
            // YOLOv8 output: [1, 84, 8400] (for 80 classes + 4 box coords)
            // or [1, dims, total_boxes]
            int dims = output.Dimensions[1];
         
            int totalBoxes = output.Dimensions[2];

            var detections = new List<DetectionResult>();

            for (int i = 0; i < totalBoxes; i++)
            {
                float maxScore = 0;
                int maxClass = -1;

                for (int c = 4; c < dims; c++)
                {
                    float score = output[0, c, i];
                    if (score > maxScore)
                    {
                        maxScore = score;
                        maxClass = c - 4;
                    }
                }

                if (maxScore > 0.3f)
                {
                    float x = output[0, 0, i] / ratio;
                    float y = output[0, 1, i] / ratio;
                    float w = output[0, 2, i] / ratio;
                    float h = output[0, 3, i] / ratio;

                    detections.Add(new DetectionResult
                    {
                        Box = new Rect((int)(x - w / 2), (int)(y - h / 2), (int)w, (int)h),
                        Score = maxScore,
                        Label = _labels[maxClass],
                        ClassIndex = maxClass
                    });
                }
            }
           
            
            return ApplyNMS(detections);
        }

        private List<DetectionResult> ApplyNMS(List<DetectionResult> detections)
        {
            // Simple NMS
            var result = new List<DetectionResult>();
            
            var sorted = detections.OrderByDescending(d => d.Score).ToList();

            while (sorted.Count > 0)
            {
                var best = sorted[0];
                result.Add(best);
                sorted.RemoveAt(0);

                sorted.RemoveAll(d => CalculateIoU(best.Box, d.Box) > 0.45f);
            }
            return result;
        }

        private float CalculateIoU(Rect box1, Rect box2)
        {
            int x1 = Math.Max(box1.X, box2.X);
            int y1 = Math.Max(box1.Y, box2.Y);
            int x2 = Math.Min(box1.Right, box2.Right);
            int y2 = Math.Min(box1.Bottom, box2.Bottom);

            int intersection = Math.Max(0, x2 - x1) * Math.Max(0, y2 - y1);
            int union = box1.Width * box1.Height + box2.Width * box2.Height - intersection;
            return (float)intersection / union;
        }

        public void Dispose() => _session.Dispose();
    }

    public class TrOcrEngine : IDisposable
    {
        private readonly InferenceSession _encoder;
        private readonly InferenceSession _decoder;
        private readonly Dictionary<int, string> _vocab;
        private readonly int _sosTokenId = 2; // decoder_start_token_id
        private readonly int _eosTokenId = 2; // eos_token_id

        public TrOcrEngine(string encoderPath, string decoderPath, string vocabPath)
        {
            var opts = new SessionOptions();
            opts.AppendExecutionProvider_CPU(1);
            _encoder = new InferenceSession(encoderPath, opts);
            _decoder = new InferenceSession(decoderPath, opts);
            
            // Load vocab mapping from JSON
            if (System.IO.File.Exists(vocabPath))
            {
                var json = System.IO.File.ReadAllText(vocabPath);
                // Simple manual parse to avoid extra dependencies, or use a proper JSON library if available
                _vocab = ParseVocab(json);
            }
            else
            {
                // Fallback basic vocab if file missing
                _vocab = new Dictionary<int, string>();
            }
        }

        private Dictionary<int, string> ParseVocab(string json)
        {
            var dict = new Dictionary<int, string>();

            var matches = System.Text.RegularExpressions.Regex.Matches(json, "\"([^\"]+)\":(\\d+)");
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
               
                    if (int.TryParse(match.Groups[2].Value, out int id))
                    {
                       
                        dict[id] = match.Groups[1].Value;
                    }
            }
            return dict;
        }

        public string Recognize(Mat plateImage)
        {
            var pixelValues = Preprocess(plateImage);
            var encoderInputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("pixel_values", pixelValues) };
            using (var encoderResult = _encoder.Run(encoderInputs))
            {
                var lastHiddenState = encoderResult.First().AsTensor<float>();

                // Decoder logic
                var inputIds = new DenseTensor<long>(new[] { 1, 1 });
                inputIds[0, 0] = _sosTokenId;

                string result = "";
                for (int i = 0; i < 32; i++) // Max length for license plates
                {
                    var decoderInputs = new List<NamedOnnxValue>
                    {
                        NamedOnnxValue.CreateFromTensor("input_ids", inputIds),
                        NamedOnnxValue.CreateFromTensor("encoder_hidden_states", lastHiddenState)
                    };

                    using (var decoderResult = _decoder.Run(decoderInputs))
                    {
                        var logits = decoderResult.First().AsTensor<float>();

                        int nextId = GetMaxIndex(logits, i);
                        if (nextId == _eosTokenId || i == 31) break;

                        if (_vocab.TryGetValue(nextId, out string token))
                        {
                            // Clean RoBERTa tokens (remove space prefix Ġ)
                            result += token.Replace("\u0120", " ").Replace("Ġ", " ");
                        }

                        // Prepare next input
                        var nextInputIds = new DenseTensor<long>(new[] { 1, i + 2 });
                        for (int j = 0; j <= i; j++) nextInputIds[0, j] = inputIds[0, j];
                        nextInputIds[0, i + 1] = nextId;
                        inputIds = nextInputIds;
                    }
                }

                return result.Trim();
            }
        }

        private Tensor<float> Preprocess(Mat img)
        {
            using (var resized = new Mat())
            {
                Cv2.Resize(img, resized, new Size(384, 384));
                var tensor = new DenseTensor<float>(new[] { 1, 3, 384, 384 });
                for (int y = 0; y < 384; y++)
                {
                    for (int x = 0; x < 384; x++)
                    {
                        var color = resized.At<Vec3b>(y, x);
                        // Standard ViT/TrOCR normalization: mean 0.5, std 0.5
                        tensor[0, 0, y, x] = (color.Item2 / 255.0f - 0.5f) / 0.5f;
                        tensor[0, 1, y, x] = (color.Item1 / 255.0f - 0.5f) / 0.5f;
                        tensor[0, 2, y, x] = (color.Item0 / 255.0f - 0.5f) / 0.5f;
                    }
                }
                return tensor;
            }
        }

        private int GetMaxIndex(Tensor<float> logits, int step)
        {
            int vocabSize = logits.Dimensions[2];
            float max = float.MinValue;
            int idx = 0;
            for (int i = 0; i < vocabSize; i++)
            {
                float val = logits[0, step, i];
                if (val > max) { max = val; idx = i; }
            }
            return idx;
        }

        public void Dispose()
        {
            _encoder.Dispose();
            _decoder.Dispose();
        }
    }

    public class DetectionResult
    {
        public Rect Box { get; set; }
        public float Score { get; set; }
        public string Label { get; set; }
        public int ClassIndex { get; set; }
    }
}
