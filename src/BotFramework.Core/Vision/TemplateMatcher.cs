using System.Drawing;

using Microsoft.Extensions.Logging;

using OpenCvSharp;
using OpenCvSharp.Extensions;

using static System.Net.Mime.MediaTypeNames;
using static OpenCvSharp.ML.DTrees;

using Point = System.Drawing.Point;

namespace BotFramework.Core.Vision
{
    /// <summary>
    /// Provides template matching functionality for UI detection using OpenCV.
    /// Supports ROI (Region of Interest) based matching with configurable thresholds.
    /// </summary>
    public class TemplateMatcher(IScreenCapturer screenCapturer, string? debugSavePath = default)
    {
        private readonly IScreenCapturer _screenCapturer = screenCapturer;
        private readonly string? _debugSavePath = debugSavePath;

        private static readonly Scalar _noLowerBoundary = new(0, 0, 0);
        private static readonly Scalar _defaultLowerBoundary = new(0, 30, 200);
        private static readonly Scalar _defaultUpperBoundary = new(255, 255, 255);

        /// <summary>
        /// Fully parameterized template matchins.
        /// </summary>
        public (Point Location, double Similarity) MatchTemplate(Bitmap image, Bitmap template, Scalar lowerBoundary, Scalar upperBoundary)
        {
            SaveDebug("image.png", image);
            SaveDebug("template.png", template);

            using var grayImage = NormalizeImage(image, lowerBoundary, upperBoundary);
            SaveDebug("grayImage.png", grayImage);


            using var grayTemplate = NormalizeImage(template, lowerBoundary, upperBoundary);
            SaveDebug("grayTemplate.png", grayTemplate);

            using var result = new Mat();
            Cv2.MatchTemplate(grayImage, grayTemplate, result, TemplateMatchModes.CCoeffNormed);
            if(!string.IsNullOrEmpty(_debugSavePath))
            {
                using var heatmap = new Mat();
                Cv2.Normalize(result, heatmap, 0, 255, NormTypes.MinMax, MatType.CV_8U);
                SaveDebug("result.png", heatmap);
            }

            Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out OpenCvSharp.Point maxLoc);

            return (new Point(maxLoc.X, maxLoc.Y), maxVal);
        }

        public (Point Location, double Similarity) MatchTemplate(Bitmap image, Bitmap template, bool standardBounds = true)
        {
            return MatchTemplate(image, template, standardBounds ? _defaultLowerBoundary : _noLowerBoundary, _defaultUpperBoundary);
        }

        /// <summary>
        /// Check if a template exists within its ROI region without color filtering.
        /// Uses full HSV range for matching.
        /// </summary>
        public bool CheckTemplate(Rectangle region, Bitmap template, double threshold, bool standardBounds = true)
        {
            var (_, Similarity) = MatchTemplate(region, template, standardBounds);

            return Similarity >= threshold;
        }

        public (Point Location, double Similarity) MatchTemplate(Rectangle region, Bitmap template, bool standardBounds = true)
        {
            using var image = _screenCapturer.CaptureBitmap(region);
            return MatchTemplate(image, template, standardBounds);
        }

        private Mat NormalizeImage(Bitmap bitmap, Scalar lowerBoundary, Scalar upperBoundary)
        {
            using var image = BitmapConverter.ToMat(bitmap);
            using var hsv = new Mat();
            using var mask = new Mat();
            using var masked = new Mat();

            Cv2.CvtColor(image, hsv, ColorConversionCodes.BGR2HSV);
            Cv2.InRange(hsv, lowerBoundary, upperBoundary, mask);
            SaveDebug("mask.png", mask);

            Cv2.BitwiseAnd(image, image, masked, mask);
            SaveDebug("masked.png", masked);

            var gray = new Mat();
            Cv2.CvtColor(masked, gray, ColorConversionCodes.BGR2GRAY);
            return gray;
        }

        private void SaveDebug(string filename, Mat image)
        {
            if (string.IsNullOrEmpty(_debugSavePath))
            {
                return;
            }

            var filePath = Path.Join(_debugSavePath, filename);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

            Cv2.ImWrite(filePath.Replace('\\', '/'), image);
        }

        private void SaveDebug(string filename, Bitmap image)
        {
            if (string.IsNullOrEmpty(_debugSavePath))
            {
                return;
            }

            var filePath = Path.Join(_debugSavePath, filename);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

            image.Save(filePath);
        }
    }
}
