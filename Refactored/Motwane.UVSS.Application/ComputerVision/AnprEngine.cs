using System;
using System.Collections.Generic;
using System.IO;
using OpenCvSharp;

namespace Motwane.UVSS.Application.ComputerVision
{
    public class AnprEngine : IDisposable
    {
        private readonly YoloDetector _detector;
        private readonly TrOcrEngine _ocrEngine;
        private readonly string _outputPath;

        public AnprEngine(string yoloModelPath, string ocrEncoderPath, string ocrDecoderPath, string vocabPath, string outputPath)
        {
            _detector = new YoloDetector(yoloModelPath, new[] { "plate", "logo", "brand" }); // Adjust labels as per model
            _ocrEngine = new TrOcrEngine(ocrEncoderPath, ocrDecoderPath, vocabPath);
            _outputPath = outputPath;

            if (!Directory.Exists(_outputPath))
                Directory.CreateDirectory(_outputPath);
        }

        public ComputerVisionResult ProcessImage(string imagePath)
        {
            var result = new ComputerVisionResult();
            try
            {
                using (var image = Cv2.ImRead(imagePath))
                {
                    if (image.Empty())
                        throw new Exception("Could not load image.");

                    // Pass original image directly — YoloDetector handles letterboxing
                    // and scales detection coords back to original image space
                    var detections = _detector.Detect(image);

                    foreach (var detection in detections)
                    {
                        // Clamp box to image bounds before cropping
                        var box = new Rect(
                            Math.Max(0, detection.Box.X),
                            Math.Max(0, detection.Box.Y),
                            Math.Min(detection.Box.Width, image.Width - Math.Max(0, detection.Box.X)),
                            Math.Min(detection.Box.Height, image.Height - Math.Max(0, detection.Box.Y))
                        );

                        using (var crop = new Mat(image, box))
                        {
                            string label = detection.Label.ToLower();
                            string timestamp = DateTime.Now.ToString("ssfff");
                            string cropPath = Path.Combine(_outputPath, $"{label}_{timestamp}.jpg");
                            crop.SaveImage(cropPath);

                            if (label.Contains("plate") || detection.ClassIndex == 0)
                            {
                                result.PlateCropPath = cropPath;
                                result.RegistrationNumber = _ocrEngine.Recognize(crop);
                            }
                            else if (label.Contains("logo"))
                            {
                                result.LogoCropPath = cropPath;
                            }
                            else if (label.Contains("brand"))
                            {
                                result.BrandName = detection.Label.ToUpper();
                            }
                        }
                    }

                    // Fallback: if no plate label matched, use first detection
                    if (string.IsNullOrEmpty(result.RegistrationNumber) && detections.Count > 0)
                    {
                        var box = new Rect(
                            Math.Max(0, detections[0].Box.X),
                            Math.Max(0, detections[0].Box.Y),
                            Math.Min(detections[0].Box.Width, image.Width - Math.Max(0, detections[0].Box.X)),
                            Math.Min(detections[0].Box.Height, image.Height - Math.Max(0, detections[0].Box.Y))
                        );
                        using (var crop = new Mat(image, box))
                        {
                            result.RegistrationNumber = _ocrEngine.Recognize(crop);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;

            }

            return result;
        }

        public void Dispose()
        {
            _detector?.Dispose();
            _ocrEngine?.Dispose();
        }
    }
}
