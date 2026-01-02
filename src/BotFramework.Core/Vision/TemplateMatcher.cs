using System.Drawing;
using System.Drawing.Imaging.Effects;

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
        /// Template matching with alpha channel support.
        /// Template pixels with alpha >= 128 (50%) are included in matching, others are ignored.
        /// Implements correlation coefficient normalized matching (CCoeffNormed) with masking.
        /// </summary>
        public (Point Location, double Similarity) MatchTemplate(Bitmap image, Bitmap template, Scalar lowerBoundary, Scalar upperBoundary, bool grayscale, bool maskAlpha)
        {
            SaveDebug("template.png", template);
            SaveDebug("image.png", image);

            using var normalTemplate = NormalizeImage(template, lowerBoundary, upperBoundary, grayscale);
            SaveDebug("normalTemplate.png", normalTemplate);

            using var normalImage = NormalizeImage(image, lowerBoundary, upperBoundary, grayscale);
            SaveDebug("normalImage.png", normalImage);

            // Extract alpha mask from template before normalization
            using var templateMat = BitmapConverter.ToMat(template);
            using Mat mask = ExtractMask(templateMat, maskAlpha);
            SaveDebug("templateMask.png", mask);

            using var result = MaskedMatchTemplate(normalImage, normalTemplate, mask);

            if (!string.IsNullOrEmpty(_debugSavePath))
            {
                using var heatmap = new Mat();
                Cv2.Normalize(result, heatmap, 0, 255, NormTypes.MinMax, MatType.CV_8U);
                SaveDebug("result.png", heatmap);
            }

            Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out OpenCvSharp.Point maxLoc);

            return (new Point(maxLoc.X, maxLoc.Y), maxVal);
        }

        public (Point Location, double Similarity) MatchTemplate(Bitmap image, Bitmap template, bool standardBounds = true, bool grayscale = true, bool maskAlpha = true)
        {
            return MatchTemplate(image, template, standardBounds ? _defaultLowerBoundary : _noLowerBoundary, _defaultUpperBoundary, grayscale, maskAlpha);
        }

        /// <summary>
        /// Check if a template exists within its ROI region without color filtering.
        /// Uses full HSV range for matching.
        /// </summary>
        public bool CheckTemplate(Rectangle region, Bitmap template, double threshold, bool standardBounds = true, bool maskAlpha = true)
        {
            var (_, Similarity) = MatchTemplate(region, template, standardBounds, maskAlpha);

            return Similarity >= threshold;
        }

        public (Point Location, double Similarity) MatchTemplate(Rectangle region, Bitmap template, bool standardBounds = true, bool maskAlpha = true)
        {
            using var image = _screenCapturer.CaptureBitmap(region);
            return MatchTemplate(image, template, standardBounds, maskAlpha: maskAlpha);
        }

        /// <summary>
        /// Extracts alpha mask from BGRA image. Pixels with alpha >= 128 are set to 255, others to 0.
        /// </summary>
        private Mat ExtractMask(Mat bgraImage, bool maskAlpha)
        {
            if (!maskAlpha || bgraImage.Channels() != 4)
            {
                // No alpha channel, return full mask
                return Mat.Ones(bgraImage.Rows, bgraImage.Cols, MatType.CV_8UC1) * 255;
            }

            var channels = Cv2.Split(bgraImage);
            var alphaMask = channels[3]; // Alpha is the 4th channel

            // Threshold at 50% (128)
            Cv2.Threshold(alphaMask, alphaMask, 127, 255, ThresholdTypes.Binary);

            // Dispose other channels
            for (int i = 0; i < 3; i++)
            {
                channels[i].Dispose();
            }

            return alphaMask;
        }

        /// <summary>
        /// Implements masked correlation coefficient normalized template matching.
        /// Only pixels where mask is non-zero are included in the calculation.
        /// </summary>
        private Mat MaskedMatchTemplate(Mat image, Mat template, Mat? mask = default)
        {
            int resultCols = image.Cols - template.Cols + 1;
            int resultRows = image.Rows - template.Rows + 1;
            var result = new Mat(resultRows, resultCols, MatType.CV_32FC1, Scalar.All(0));

            if (resultCols <= 0 || resultRows <= 0)
            {
                return result;
            }

            // if mask == null, create full mask
            using var fullMask = Mat.Ones(template.Rows, template.Cols, MatType.CV_8UC1) * 255;
            mask ??= fullMask;

            // Calculate template statistics under mask
            double templateSum = 0;
            double templateSqSum = 0;
            int maskCount = 0;

            if (template.Channels() == 1)
            {
                // Grayscale
                for (int y = 0; y < template.Rows; y++)
                {
                    for (int x = 0; x < template.Cols; x++)
                    {
                        if (mask.At<byte>(y, x) > 0)
                        {
                            double val = template.At<byte>(y, x);
                            templateSum += val;
                            templateSqSum += val * val;
                            maskCount++;
                        }
                    }
                }
            }
            else
            {
                // Color (BGR)
                for (int y = 0; y < template.Rows; y++)
                {
                    for (int x = 0; x < template.Cols; x++)
                    {
                        if (mask.At<byte>(y, x) > 0)
                        {
                            Vec3b pixel = template.At<Vec3b>(y, x);
                            double val = (pixel.Item0 + pixel.Item1 + pixel.Item2) / 3.0; // Average channels
                            templateSum += val;
                            templateSqSum += val * val;
                            maskCount++;
                        }
                    }
                }
            }

            if (maskCount == 0)
            {
                return result; // No valid pixels in mask
            }

            double templateMean = templateSum / maskCount;

            // Slide template across image
            for (int y = 0; y < resultRows; y++)
            {
                for (int x = 0; x < resultCols; x++)
                {
                    double imageSum = 0;
                    double imageSqSum = 0;
                    double crossSum = 0;

                    if (template.Channels() == 1)
                    {
                        // Grayscale
                        for (int ty = 0; ty < template.Rows; ty++)
                        {
                            for (int tx = 0; tx < template.Cols; tx++)
                            {
                                if (mask.At<byte>(ty, tx) > 0)
                                {
                                    double templateVal = template.At<byte>(ty, tx);
                                    double imageVal = image.At<byte>(y + ty, x + tx);

                                    imageSum += imageVal;
                                    imageSqSum += imageVal * imageVal;
                                    crossSum += templateVal * imageVal;
                                }
                            }
                        }
                    }
                    else
                    {
                        // Color (BGR)
                        for (int ty = 0; ty < template.Rows; ty++)
                        {
                            for (int tx = 0; tx < template.Cols; tx++)
                            {
                                if (mask.At<byte>(ty, tx) > 0)
                                {
                                    Vec3b templatePixel = template.At<Vec3b>(ty, tx);
                                    Vec3b imagePixel = image.At<Vec3b>(y + ty, x + tx);

                                    double templateVal = (templatePixel.Item0 + templatePixel.Item1 + templatePixel.Item2) / 3.0;
                                    double imageVal = (imagePixel.Item0 + imagePixel.Item1 + imagePixel.Item2) / 3.0;

                                    imageSum += imageVal;
                                    imageSqSum += imageVal * imageVal;
                                    crossSum += templateVal * imageVal;
                                }
                            }
                        }
                    }

                    double imageMean = imageSum / maskCount;

                    // Correlation coefficient normalized formula
                    double numerator = crossSum - maskCount * templateMean * imageMean;
                    double denomTemplate = templateSqSum - maskCount * templateMean * templateMean;
                    double denomImage = imageSqSum - maskCount * imageMean * imageMean;

                    double correlation = 0;
                    if (denomTemplate > 0 && denomImage > 0)
                    {
                        correlation = numerator / Math.Sqrt(denomTemplate * denomImage);
                    }

                    result.Set(y, x, (float)correlation);
                }
            }

            return result;
        }

        private Mat NormalizeImage(Bitmap bitmap, Scalar lowerBoundary, Scalar upperBoundary, bool grayscale)
        {
            using var image = BitmapConverter.ToMat(bitmap);
            // Ensure consistent channel count - convert BGRA to BGR if needed
            if (image.Channels() == 4)
            {
                Cv2.CvtColor(image, image, ColorConversionCodes.BGRA2BGR);
            }

            using var hsv = new Mat();
            using var mask = new Mat();
            using var masked = new Mat();

            Cv2.CvtColor(image, hsv, ColorConversionCodes.BGR2HSV);
            Cv2.InRange(hsv, lowerBoundary, upperBoundary, mask);
            SaveDebug("mask.png", mask);

            Cv2.BitwiseAnd(image, image, masked, mask);
            SaveDebug("masked.png", masked);

            var result = new Mat();

            if (grayscale)
            {
                Cv2.CvtColor(masked, result, ColorConversionCodes.BGR2GRAY);
            }
            else
            {
                result = masked.Clone();
            }

            return result;
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
