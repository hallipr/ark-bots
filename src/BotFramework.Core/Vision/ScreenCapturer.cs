using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace BotFramework.Core.Vision
{
    /// <summary>
    /// Provides screen capture functionality for detecting game state through image recognition.
    /// Supports 1080p and 1440p resolutions with automatic scaling.
    /// </summary>
    public class ScreenCapturer : IScreenCapturer
    {
        private readonly IProcessMonitor _processMonitor;

        public ScreenCapturer(IProcessMonitor processMonitor)
        {
            _processMonitor = processMonitor;
        }

        public int ScreenHeight => ScreenRectangle.Height;

        public Rectangle ScreenRectangle => GetScreenRectangle();

        protected virtual Rectangle GetScreenRectangle()
        {
            if (GetWindowRect(_processMonitor.HWnd, out RECT rect))
            {
                int height = rect.Bottom - rect.Top;
                int width = rect.Right - rect.Left;
                return new Rectangle(rect.Left, rect.Top, width, height);
            }

            throw new InvalidOperationException("Could not determine screen resolution");
        }

        /// <summary>
        /// Captures a region of interest (ROI) from the screen.
        /// </summary>
        /// <param name="startX">X coordinate of top-left corner</param>
        /// <param name="startY">Y coordinate of top-left corner</param>
        /// <param name="width">Width of the region</param>
        /// <param name="height">Height of the region</param>
        /// <returns>Bitmap of the captured region</returns>
        public Bitmap CaptureBitmap(Rectangle region)
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
