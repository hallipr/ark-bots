using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;

namespace ArkBotFramework.ArkWindow
{
    /// <summary>
    /// Manages window interaction and provides mouse/keyboard input control.
    /// Uses Windows API for precise input simulation.
    /// </summary>
    public class WindowManager
    {
        private readonly IntPtr _hwnd;
        private readonly Rectangle _monitor;

        /// <summary>
        /// Gets the window handle for the target application.
        /// </summary>
        public IntPtr Hwnd => _hwnd;

        // Mouse constants
        private const int INPUT_MOUSE = 0;
        private const int MOUSEEVENTF_MOVE = 0x0001;
        private const int MOUSEEVENTF_MOVE_NOCOALESCE = 0x2000;
        private const int MOUSEEVENTF_ABSOLUTE = 0x8000;
        private const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const int MOUSEEVENTF_LEFTUP = 0x0004;

        // Message constants
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONUP = 0x0202;

        // Camera sensitivity constants (max values)
        private const double PIXELS_PER_DEGREE = 128.6 / 90.0;
        private const double MAX_LR_SENS = 3.2;
        private const double MAX_UD_SENS = 3.2;
        private const double MAX_FOV = 1.25;

        public WindowManager(string windowTitle, Rectangle monitor)
        {
            _hwnd = FindWindowByTitle(windowTitle);
            _monitor = monitor;

            if (_hwnd == IntPtr.Zero)
            {
                throw new InvalidOperationException($"Could not find window with title: {windowTitle}");
            }
        }

        /// <summary>
        /// Finds a window by its title.
        /// </summary>
        /// <param name="title">Window title to search for</param>
        /// <returns>Window handle or IntPtr.Zero if not found</returns>
        public static IntPtr FindWindowByTitle(string title)
        {
            return FindWindowW(IntPtr.Zero, title);
        }

        /// <summary>
        /// Simulates camera movement with sensitivity and FOV compensation.
        /// </summary>
        /// <param name="x">Horizontal degrees to turn</param>
        /// <param name="y">Vertical degrees to turn</param>
        /// <param name="lookLrSens">Player's left-right sensitivity setting</param>
        /// <param name="lookUdSens">Player's up-down sensitivity setting</param>
        /// <param name="fov">Player's FOV setting</param>
        public void Turn(int x, int y, double lookLrSens, double lookUdSens, double fov)
        {
            int dx = (int)Math.Round(x * PIXELS_PER_DEGREE * (MAX_LR_SENS / lookLrSens) * (MAX_FOV / fov));
            int dy = (int)Math.Round(y * PIXELS_PER_DEGREE * (MAX_UD_SENS / lookUdSens) * (MAX_FOV / fov));

            var input = new INPUT
            {
                type = INPUT_MOUSE,
                mi = new MOUSEINPUT
                {
                    dx = dx,
                    dy = dy,
                    mouseData = 0,
                    dwFlags = MOUSEEVENTF_MOVE | MOUSEEVENTF_MOVE_NOCOALESCE,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            };

            SendInput(1, new[] { input }, Marshal.SizeOf(typeof(INPUT)));
        }

        /// <summary>
        /// Moves the mouse cursor to absolute screen coordinates.
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        public void MoveMouse(int x, int y)
        {
            int scaledX = (int)(x * 65535 / _monitor.Width);
            int scaledY = (int)(y * 65535 / _monitor.Height);

            mouse_event(MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE, scaledX, scaledY, 0, IntPtr.Zero);
        }

        /// <summary>
        /// Simulates a mouse click at the specified coordinates.
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        public void Click(int x, int y)
        {
            MoveMouse(x, y);
            Thread.Sleep(50); // Small delay to ensure mouse is positioned

            // Send left button down
            PostMessageW(_hwnd, WM_LBUTTONDOWN, IntPtr.Zero, MakeLParam(x, y));
            Thread.Sleep(20);

            // Send left button up
            PostMessageW(_hwnd, WM_LBUTTONUP, IntPtr.Zero, MakeLParam(x, y));
            Thread.Sleep(20);
        }

        /// <summary>
        /// Creates an LPARAM value from x and y coordinates.
        /// </summary>
        private IntPtr MakeLParam(int x, int y)
        {
            return (IntPtr)((y << 16) | (x & 0xFFFF));
        }

        #region Win32 API Imports and Structures

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr FindWindowW(IntPtr hWndParent, string lpClassName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, IntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern bool PostMessageW(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public int type;
            public MOUSEINPUT mi;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public int dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        #endregion
    }
}
