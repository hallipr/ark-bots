using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ArkBotFramework.ArkWindow
{
    /// <summary>
    /// Provides screen capture functionality for detecting game state through image recognition.
    /// Supports 1080p and 1440p resolutions with automatic scaling.
    /// </summary>
    public class ScreenCapture
    {
        private readonly IntPtr _windowHandle;
        private int _screenResolution;
        private Rectangle _monitor;

        /// <summary>
        /// Gets the detected screen resolution (1080 or 1440).
        /// </summary>
        public int ScreenResolution => _screenResolution;

        /// <summary>
        /// Gets the monitor bounds for the target window.
        /// </summary>
        public Rectangle Monitor => _monitor;

        public ScreenCapture(IntPtr windowHandle)
        {
            _windowHandle = windowHandle;
            _screenResolution = FindScreenSize();
            InitializeMonitor();
        }

        /// <summary>
        /// Detects the screen resolution by measuring the window height.
        /// </summary>
        /// <returns>Screen height (1080 or 1440)</returns>
        private int FindScreenSize()
        {
            if (GetWindowRect(_windowHandle, out RECT rect))
            {
                int height = rect.Bottom - rect.Top;
                Console.WriteLine($"Using {height} as screen resolution");
                return height;
            }

            throw new InvalidOperationException("Could not determine screen resolution");
        }

        /// <summary>
        /// Initializes monitor bounds based on detected resolution.
        /// </summary>
        private void InitializeMonitor()
        {
            if (_screenResolution == 1080)
            {
                _monitor = new Rectangle(0, 0, 1920, 1080);
            }
            else if (_screenResolution == 1440)
            {
                _monitor = new Rectangle(0, 0, 2560, 1440);
            }
            else
            {
                throw new InvalidOperationException(
                    $"{_screenResolution} is not a valid screen resolution. It needs to be 1920x1080 or 2560x1440");
            }
        }

        /// <summary>
        /// Captures a region of interest (ROI) from the screen.
        /// </summary>
        /// <param name="startX">X coordinate of top-left corner</param>
        /// <param name="startY">Y coordinate of top-left corner</param>
        /// <param name="width">Width of the region</param>
        /// <param name="height">Height of the region</param>
        /// <returns>Bitmap of the captured region</returns>
        public Bitmap GetScreenRoi(int startX, int startY, int width, int height)
        {
            var rect = new Rectangle(startX, startY, width, height);
            return CaptureRegion(rect);
        }

        /// <summary>
        /// Captures a specific rectangle region from the screen.
        /// </summary>
        private Bitmap CaptureRegion(Rectangle region)
        {
            var bitmap = new Bitmap(region.Width, region.Height);
            
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(
                    region.X,
                    region.Y,
                    0,
                    0,
                    region.Size);
            }

            return bitmap;
        }

        /// <summary>
        /// Gets the scaling factor based on resolution.
        /// Used for coordinate translation between 1080p and 1440p.
        /// </summary>
        /// <returns>Scaling factor (0.75 for 1080p, 1.0 for 1440p)</returns>
        public double GetScalingFactor()
        {
            return _screenResolution == 1080 ? 0.75 : 1.0;
        }

        /// <summary>
        /// Scales a coordinate value based on the current resolution.
        /// </summary>
        public int ScaleCoordinate(int value)
        {
            return (int)Math.Round(value * GetScalingFactor());
        }

        #region Win32 API Imports

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        #endregion
    }
}
