using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Motwane.UVSS.Application.Interfaces.HAL;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using WMPLib;

namespace Motwane.UVSS.Infrastructure.HAL.ExternalServices
{
    public class AicComparisonService : IAicComparisonService
    {
        WindowsMediaPlayer player = new WindowsMediaPlayer();
        Stopwatch sw = new Stopwatch();

        public byte[] PerformComparison(string firstImagePth, string secondImagePth)
        {
            if (File.Exists(firstImagePth) && File.Exists(secondImagePth))
            {
                var img1 = new Image<Bgr, byte>(firstImagePth);
                var img2 = new Image<Bgr, byte>(secondImagePth);

                if (img1.Size != img2.Size)
                    img2 = img2.Resize(img1.Width, img1.Height, Inter.Linear);

                var gray1 = img1.Convert<Gray, byte>();
                var gray2 = img2.Convert<Gray, byte>();

                var diff = gray1.AbsDiff(gray2);
                var thresh = diff.ThresholdBinary(new Gray(80), new Gray(255));

                CvInvoke.MorphologyEx(
                    thresh,
                    thresh,
                    MorphOp.Open,
                    CvInvoke.GetStructuringElement(
                        ElementShape.Rectangle,
                        new Size(7, 7),
                        new Point(-1, -1)),
                    new Point(-1, -1),
                    1,
                    BorderType.Default,
                    new MCvScalar());

                var contours = new VectorOfVectorOfPoint();

                CvInvoke.FindContours(
                    thresh,
                    contours,
                    null,
                    RetrType.External,
                    ChainApproxMethod.ChainApproxSimple);

                for (int i = 0; i < contours.Size; i++)
                {
                    var rect = CvInvoke.BoundingRectangle(contours[i]);

                    if (rect.Width > 60 && rect.Height > 60)
                    {
                        CvInvoke.Rectangle(img2, rect, new MCvScalar(0, 0, 255), 2);
                    }
                }

                if (contours.Size > 15)
                {
                    player.URL = @"D:\alarm_tunes\Tune 1.mp3";
                    player.controls.play();
                    sw = Stopwatch.StartNew();
                }

                using (VectorOfByte vb = new VectorOfByte())
                {
                    CvInvoke.Imencode(".bmp", img2, vb);
                    return vb.ToArray();
                }
            }

            return null;
        }

        public byte[] LoadImageUnlocked(string path)
        {
            return File.ReadAllBytes(path);
        }
    }
}