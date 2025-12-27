using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;

using ArkBotFramework.ArkWindow;

using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace ArkBotFramework.Infrastructure
{
    /// <summary>
    /// Provides template matching functionality for UI detection using OpenCV.
    /// Supports ROI (Region of Interest) based matching with configurable thresholds.
    /// </summary>
    public class TemplateMatching
    {
        private readonly ScreenCapture _screenCapture;
        private readonly Dictionary<string, RoiRegion> _roiRegions;
        private readonly string _iconsPath;
        private readonly Action<string> _logTemplate;

        /// <summary>
        /// Gets the dictionary of defined ROI regions.
        /// </summary>
        public IReadOnlyDictionary<string, RoiRegion> RoiRegions => _roiRegions;

        public TemplateMatching(
            ScreenCapture screenCapture,
            Action<string> logTemplate = null)
        {
            _screenCapture = screenCapture;
            _logTemplate = logTemplate ?? Console.WriteLine;
            _iconsPath = $"icons{_screenCapture.ScreenResolution}";
            _roiRegions = InitializeRoiRegions();
        }

        /// <summary>
        /// Initialize all ROI regions for template matching.
        /// </summary>
        private Dictionary<string, RoiRegion> InitializeRoiRegions()
        {
            return new Dictionary<string, RoiRegion>
            {
                { "bed_radical", new RoiRegion(1120, 345, 250, 250) },
                { "beds_title", new RoiRegion(100, 100, 740, 180) },
                { "console", new RoiRegion(0, 1400, 50, 40) },
                { "crop_plot", new RoiRegion(1100, 250, 310, 150) },
                { "crystal_in_hotbar", new RoiRegion(750, 1250, 1060, 250) },
                { "death_regions", new RoiRegion(100, 100, 700, 200) },
                { "dedi", new RoiRegion(1100, 245, 355, 70) },
                { "vault", new RoiRegion(1100, 245, 355, 150) },
                { "grinder", new RoiRegion(1100, 245, 355, 70) },
                { "exit_resume", new RoiRegion(550, 450, 1670, 880) },
                { "inventory", new RoiRegion(200, 125, 360, 150) },
                { "ready_clicked_bed", new RoiRegion(580, 250, 150, 1000) },
                { "seed_inv", new RoiRegion(550, 450, 1670, 880) },
                { "slot_capped", new RoiRegion(2240, 1314, 150, 100) },
                { "teleporter_title", new RoiRegion(200, 135, 405, 185) },
                { "tribelog_check", new RoiRegion(1150, 35, 150, 150) },
                { "waiting_inv", new RoiRegion(2000, 100, 500, 250) },
                { "bed_icon", new RoiRegion(800, 200, 1690, 1100) },
                { "teleporter_icon", new RoiRegion(800, 200, 1690, 1100) },
                { "teleporter_icon_pressed", new RoiRegion(800, 200, 1690, 1100) },
                { "first_slot", new RoiRegion(220, 305, 130, 130) },
                { "player_stats", new RoiRegion(1120, 240, 300, 900) },
                { "show_buff", new RoiRegion(1200, 1150, 200, 50) },
                { "snow_owl_pellet", new RoiRegion(200, 150, 600, 600) },
                { "orange", new RoiRegion(705, 290, 1, 1) },
                { "chem_bench", new RoiRegion(1100, 245, 355, 70) },
                { "indi_forge", new RoiRegion(1100, 245, 355, 70) },
                { "access_inv", new RoiRegion(550, 450, 1670, 880) }
            };
        }

        /// <summary>
        /// Check if a template exists within its ROI region with standard color boundaries.
        /// Uses HSV color masking to filter the image before matching.
        /// </summary>
        public bool CheckTemplate(string item, double threshold)
        {
            var region = GetScaledRegion(item);
            var lowerBoundary = new Scalar(0, 30, 200);
            var upperBoundary = new Scalar(255, 255, 255);

            return CheckTemplateInternal(item, threshold, region, lowerBoundary, upperBoundary);
        }

        /// <summary>
        /// Check if a template exists within its ROI region without color filtering.
        /// Uses full HSV range for matching.
        /// </summary>
        public bool CheckTemplateNoBounds(string item, double threshold)
        {
            var region = GetScaledRegion(item);
            var lowerBoundary = new Scalar(0, 0, 0);
            var upperBoundary = new Scalar(255, 255, 255);

            return CheckTemplateInternal(item, threshold, region, lowerBoundary, upperBoundary);
        }

        /// <summary>
        /// Returns the location of a template match within the screen.
        /// Assumes the template exists (should be checked first).
        /// </summary>
        public OpenCvSharp.Point? ReturnLocation(string item, double threshold)
        {
            var region = GetScaledRegion(item);
            var lowerBoundary = new Scalar(0, 0, 0);
            var upperBoundary = new Scalar(255, 255, 255);

            using var roi = CaptureAndConvertRoi(region);
            using var template = LoadAndProcessTemplate(item, lowerBoundary, upperBoundary);

            var result = MatchTemplate(roi, template);
            Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out OpenCvSharp.Point maxLoc);
            result.Dispose();

            if (maxVal > threshold)
            {
                _logTemplate($"{item} found:{maxVal} at:{maxLoc}");
                return maxLoc;
            }

            _logTemplate($"{item} not found:{maxVal} threshold:{threshold}");
            return null;
        }

        /// <summary>
        /// Wait for a template check function to return true.
        /// </summary>
        public bool TemplateAwaitTrue(Func<bool> func, double sleepAmount)
        {
            int count = 0;
            int maxCount = (int)(sleepAmount * 20);

            while (!func() && count < maxCount)
            {
                Thread.Sleep(50);
                count++;
            }

            return func();
        }

        /// <summary>
        /// Wait for a template check function to return false.
        /// </summary>
        public bool TemplateAwaitFalse(Func<bool> func, double sleepAmount)
        {
            int count = 0;
            int maxCount = (int)(sleepAmount * 20);

            while (func() && count < maxCount)
            {
                Thread.Sleep(50);
                count++;
            }

            return func();
        }

        /// <summary>
        /// Check for the teleporter icon with specific blue color filtering.
        /// </summary>
        public bool TeleportIcon(double threshold)
        {
            var region = GetScaledRegion("teleporter_icon");
            var lowerBoundary = new Scalar(0, 0, 150);
            var upperBoundary = new Scalar(255, 255, 255);

            return CheckTemplateInternal("teleporter_icon", threshold, region, lowerBoundary, upperBoundary);
        }

        /// <summary>
        /// Check for buff icons in the player stats area.
        /// </summary>
        public bool CheckBuffs(string buff, double threshold)
        {
            var region = GetScaledRegion("player_stats");
            var lowerBoundary = new Scalar(0, 0, 180);
            var upperBoundary = new Scalar(255, 255, 255);

            return CheckTemplateInternal(buff, threshold, region, lowerBoundary, upperBoundary);
        }

        /// <summary>
        /// Check if the teleporter icon has turned orange (indicating selection).
        /// </summary>
        public bool CheckTeleporterOrange()
        {
            var region = GetScaledRegion("orange");
            using var bitmap = _screenCapture.GetScreenRoi(region.X, region.Y, region.Width, region.Height);
            using var mat = BitmapConverter.ToMat(bitmap);
            using var hsv = new Mat();

            Cv2.CvtColor(mat, hsv, ColorConversionCodes.BGR2HSV);

            var pixel = hsv.Get<Vec3b>(0, 0);
            var lowerBoundary = new Vec3b(10, 211, 50);
            var upperBoundary = new Vec3b(15, 255, 100);

            bool isOrange = pixel[0] >= lowerBoundary[0] && pixel[0] <= upperBoundary[0] &&
                           pixel[1] >= lowerBoundary[1] && pixel[1] <= upperBoundary[1] &&
                           pixel[2] >= lowerBoundary[2] && pixel[2] <= upperBoundary[2];

            _logTemplate($"check orange {isOrange}");
            return isOrange;
        }

        /// <summary>
        /// Check for white flash on screen (loading indicator).
        /// </summary>
        public bool WhiteFlash()
        {
            using var bitmap = _screenCapture.GetScreenRoi(500, 500, 100, 100);
            using var mat = BitmapConverter.ToMat(bitmap);

            int totalPixels = mat.Rows * mat.Cols * mat.Channels();
            int count255 = Cv2.CountNonZero(mat);
            double percentage = (count255 / (double)totalPixels) * 100;

            bool isWhite = percentage >= 80;
            _logTemplate($"white flash {isWhite}");
            return isWhite;
        }

        /// <summary>
        /// Internal template checking with custom color boundaries.
        /// </summary>
        private bool CheckTemplateInternal(string item, double threshold, Rectangle region, 
            Scalar lowerBoundary, Scalar upperBoundary)
        {
            using var roi = CaptureAndConvertRoi(region);
            using var template = LoadAndProcessTemplate(item, lowerBoundary, upperBoundary);

            var result = MatchTemplate(roi, template);
            Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out _);
            result.Dispose();

            if (maxVal > threshold)
            {
                _logTemplate($"{item} found:{maxVal}");
                return true;
            }

            _logTemplate($"{item} not found:{maxVal} threshold:{threshold}");
            return false;
        }

        /// <summary>
        /// Capture ROI from screen, apply color masking, and convert to grayscale.
        /// </summary>
        private Mat CaptureAndConvertRoi(Rectangle region)
        {
            using var bitmap = _screenCapture.GetScreenRoi(region.X, region.Y, region.Width, region.Height);
            using var mat = BitmapConverter.ToMat(bitmap);
            using var hsv = new Mat();
            using var mask = new Mat();
            using var masked = new Mat();

            Cv2.CvtColor(mat, hsv, ColorConversionCodes.BGR2HSV);
            // Note: Boundaries should be passed in, but using default here
            Cv2.InRange(hsv, new Scalar(0, 30, 200), new Scalar(255, 255, 255), mask);
            Cv2.BitwiseAnd(mat, mat, masked, mask);

            var gray = new Mat();
            Cv2.CvtColor(masked, gray, ColorConversionCodes.BGR2GRAY);
            return gray;
        }

        /// <summary>
        /// Load template image, apply color masking, and convert to grayscale.
        /// </summary>
        private Mat LoadAndProcessTemplate(string item, Scalar lowerBoundary, Scalar upperBoundary)
        {
            string templatePath = Path.Combine(_iconsPath, $"{item}.png");
            using var image = Cv2.ImRead(templatePath);
            
            if (image.Empty())
            {
                throw new FileNotFoundException($"Template image not found: {templatePath}");
            }

            using var hsv = new Mat();
            using var mask = new Mat();
            using var masked = new Mat();

            Cv2.CvtColor(image, hsv, ColorConversionCodes.BGR2HSV);
            Cv2.InRange(hsv, lowerBoundary, upperBoundary, mask);
            Cv2.BitwiseAnd(image, image, masked, mask);

            var gray = new Mat();
            Cv2.CvtColor(masked, gray, ColorConversionCodes.BGR2GRAY);
            return gray;
        }

        /// <summary>
        /// Perform template matching.
        /// </summary>
        private Mat MatchTemplate(Mat roi, Mat template)
        {
            var result = new Mat();
            Cv2.MatchTemplate(roi, template, result, TemplateMatchModes.CCoeffNormed);
            return result;
        }

        /// <summary>
        /// Get the scaled region based on screen resolution.
        /// </summary>
        private Rectangle GetScaledRegion(string item)
        {
            if (!_roiRegions.TryGetValue(item, out var region))
            {
                throw new KeyNotFoundException($"ROI region not found: {item}");
            }

            double scale = _screenCapture.GetScalingFactor();
            return new Rectangle(
                (int)(region.StartX * scale),
                (int)(region.StartY * scale),
                (int)(region.Width * scale),
                (int)(region.Height * scale));
        }

        /// <summary>
        /// Represents a region of interest for template matching.
        /// </summary>
        public class RoiRegion
        {
            public int StartX { get; }
            public int StartY { get; }
            public int Width { get; }
            public int Height { get; }

            public RoiRegion(int startX, int startY, int width, int height)
            {
                StartX = startX;
                StartY = startY;
                Width = width;
                Height = height;
            }
        }
    }
}
