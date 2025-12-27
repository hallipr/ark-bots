using System;
using System.Drawing;
using System.Threading;

using ArkBotFramework.Infrastructure;

namespace ArkBotFramework.ArkWindow
{
    /// <summary>
    /// Handles ARK main menu navigation.
    /// Detects and interacts with the main menu to begin reconnection process.
    /// </summary>
    public class MainMenu
    {
        private readonly ScreenCapture _screenCapture;
        private readonly WindowManager _windowManager;
        private readonly TemplateMatching _templateMatching;

        // Button coordinates (1440p baseline)
        private readonly ButtonCoordinates _buttons = new ButtonCoordinates
        {
            AcceptX = 1255,
            AcceptY = 980,
            JoinLastSessionX = 1250,
            JoinLastSessionY = 1260,
            StartX = 1270,
            StartY = 1150
        };

        public MainMenu(ScreenCapture screenCapture, WindowManager windowManager, TemplateMatching templateMatching)
        {
            _screenCapture = screenCapture;
            _windowManager = windowManager;
            _templateMatching = templateMatching;
        }

        /// <summary>
        /// Check if the main menu is currently open.
        /// </summary>
        public bool IsOpen()
        {
            return _templateMatching.CheckTemplateNoBounds("join_last_session", 0.7);
        }

        /// <summary>
        /// Check if there's a disconnect error dialog.
        /// </summary>
        public bool Disconnect()
        {
            return _templateMatching.CheckTemplateNoBounds("connection_timeout", 0.7);
        }

        /// <summary>
        /// Join the last session from the main menu.
        /// </summary>
        public void JoinLast()
        {
            if (!IsOpen())
                return;

            if (Disconnect())
            {
                _windowManager.Click(GetScaledX(_buttons.AcceptX), GetScaledY(_buttons.AcceptY));
                WaitForTemplateClose("accept", 0.7, 1);
            }

            _windowManager.Click(GetScaledX(_buttons.JoinLastSessionX), GetScaledY(_buttons.JoinLastSessionY));
            WaitForTemplateClose("join_last_session", 0.7, 1);
        }

        /// <summary>
        /// Enter the main menu by clicking start button.
        /// </summary>
        public void EnterMenu()
        {
            if (!IsOpen())
                return;

            if (Disconnect())
            {
                _windowManager.Click(GetScaledX(_buttons.AcceptX), GetScaledY(_buttons.AcceptY));
                WaitForTemplateClose("accept", 0.7, 1);
            }

            // Click to dismiss any popups
            _windowManager.Click(GetScaledX(_buttons.AcceptX), GetScaledY(_buttons.AcceptY));
            Thread.Sleep(100);
            
            _windowManager.Click(GetScaledX(_buttons.StartX), GetScaledY(_buttons.StartY));
            WaitForTemplateClose("join_last_session", 0.7, 1);
        }

        private int GetScaledX(int coordinate)
        {
            return _screenCapture.ScaleCoordinate(coordinate);
        }

        private int GetScaledY(int coordinate)
        {
            return _screenCapture.ScaleCoordinate(coordinate);
        }

        private void WaitForTemplateClose(string template, double threshold, int timeoutSeconds)
        {
            _templateMatching.TemplateAwaitFalse(
                () => _templateMatching.CheckTemplateNoBounds(template, threshold),
                timeoutSeconds);
        }

        private class ButtonCoordinates
        {
            public int AcceptX { get; set; }
            public int AcceptY { get; set; }
            public int JoinLastSessionX { get; set; }
            public int JoinLastSessionY { get; set; }
            public int StartX { get; set; }
            public int StartY { get; set; }
        }
    }
}
