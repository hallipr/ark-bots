using System;
using System.Threading;

using ArkBotFramework.Infrastructure;

namespace ArkBotFramework.ArkWindow
{
    /// <summary>
    /// Handles multiplayer menu navigation for server joining.
    /// Searches for servers, handles errors, and joins selected servers.
    /// </summary>
    public class MultiplayerMenu
    {
        private readonly ScreenCapture _screenCapture;
        private readonly WindowManager _windowManager;
        private readonly TemplateMatching _templateMatching;
        private readonly Action<string> _pressKey;
        private readonly Action _ctrlA;
        private readonly Action<string> _write;

        // Button coordinates (1440p baseline)
        private readonly ButtonCoordinates _buttons = new ButtonCoordinates
        {
            SearchX = 2230, SearchY = 260,
            FirstServerX = 2230, FirstServerY = 438,
            JoinX = 2230, JoinY = 1260,
            RefreshX = 1240, RefreshY = 1250,
            BackX = 230, BackY = 1180,
            CancelX = 1426, CancelY = 970,
            RedOkayX = 1270, RedOkayY = 880,
            ModJoinX = 700, ModJoinY = 1250
        };

        public MultiplayerMenu(
            ScreenCapture screenCapture,
            WindowManager windowManager,
            TemplateMatching templateMatching,
            Action<string> pressKey,
            Action ctrlA,
            Action<string> write)
        {
            _screenCapture = screenCapture;
            _windowManager = windowManager;
            _templateMatching = templateMatching;
            _pressKey = pressKey;
            _ctrlA = ctrlA;
            _write = write;
        }

        /// <summary>
        /// Join a server by name.
        /// Handles searching, joining, and various error conditions.
        /// </summary>
        public void JoinServer(string serverName)
        {
            if (!_templateMatching.CheckTemplateNoBounds("multiplayer", 0.7))
            {
                _pressKey("ShowTribeManager");
                return;
            }

            Thread.Sleep(300);

            // Search for server
            _windowManager.Click(GetScaled(_buttons.SearchX), GetScaled(_buttons.SearchY));
            Thread.Sleep(300);
            _ctrlA();
            Thread.Sleep(200);
            _write(serverName);
            Thread.Sleep(300);

            // Select first server
            _windowManager.Click(GetScaled(_buttons.FirstServerX), GetScaled(_buttons.FirstServerY));

            // Join server if available
            if (_templateMatching.CheckTemplateNoBounds("join_button", 0.7))
            {
                Thread.Sleep(200);
                _windowManager.Click(GetScaled(_buttons.JoinX), GetScaled(_buttons.JoinY));
                Thread.Sleep(500);
            }

            // Wait for join dialog (up to 20 seconds)
            WaitForTemplateClose("join_text", 0.7, 20);

            // Handle mod download/join
            if (_templateMatching.CheckTemplateNoBounds("mod_join", 0.7))
            {
                if (WaitForTemplate("req_mods", 0.7, 10))
                {
                    Thread.Sleep(500);
                    _windowManager.Click(GetScaled(_buttons.ModJoinX), GetScaled(_buttons.ModJoinY));
                    Thread.Sleep(2000);
                    WaitForTemplateClose("join_text", 0.7, 20);
                    Thread.Sleep(2000);
                }
            }

            // Wait for loading screen
            if (WaitForTemplate("loading_screen", 0.7, 0.5))
            {
                WaitForTemplateClose("loading_screen", 0.7, 10);

                // Wait for tribelog to appear (with timeout)
                int count = 0;
                while (!_templateMatching.CheckTemplateNoBounds("tribelog_check", 0.8) && count < 600)
                {
                    _pressKey("ShowTribeManager");
                    Thread.Sleep(100);
                    count++;
                }

                Thread.Sleep(5000);
                return;
            }

            // Handle server full
            if (_templateMatching.CheckTemplate("server_full", 0.7))
            {
                _windowManager.Click(GetScaled(_buttons.CancelX), GetScaled(_buttons.CancelY));
                WaitForTemplateClose("server_full", 0.7, 2);
                Thread.Sleep(500);
                _windowManager.Click(GetScaled(_buttons.BackX), GetScaled(_buttons.BackY));
                return;
            }

            // Handle red error dialog
            if (_templateMatching.CheckTemplate("red_fail", 0.7))
            {
                _windowManager.Click(GetScaled(_buttons.RedOkayX), GetScaled(_buttons.RedOkayY));
                WaitForTemplateClose("red_fail", 0.7, 2);
                Thread.Sleep(500);
                _windowManager.Click(GetScaled(_buttons.BackX), GetScaled(_buttons.BackY));
                return;
            }

            // Fallback: go back
            Thread.Sleep(500);
            _windowManager.Click(GetScaled(_buttons.BackX), GetScaled(_buttons.BackY));

            // Handle searching state
            if (WaitForTemplate("searching", 0.7, 0.5))
            {
                WaitForTemplateClose("searching", 0.7, 10);
                Thread.Sleep(2000);
            }

            // Handle no session found
            if (_templateMatching.CheckTemplateNoBounds("no_session", 0.7))
            {
                Thread.Sleep(2000);
                _windowManager.Click(GetScaled(_buttons.BackX), GetScaled(_buttons.BackY));
                Thread.Sleep(2000);
            }

            // Final attempt to show tribelog (for special event screens)
            _pressKey("ShowTribeManager");
        }

        private int GetScaled(int coordinate)
        {
            return _screenCapture.ScaleCoordinate(coordinate);
        }

        private bool WaitForTemplate(string template, double threshold, double timeoutSeconds)
        {
            return _templateMatching.TemplateAwaitTrue(
                () => _templateMatching.CheckTemplateNoBounds(template, threshold),
                timeoutSeconds);
        }

        private void WaitForTemplateClose(string template, double threshold, int timeoutSeconds)
        {
            _templateMatching.TemplateAwaitFalse(
                () => _templateMatching.CheckTemplateNoBounds(template, threshold),
                timeoutSeconds);
        }

        private class ButtonCoordinates
        {
            public int SearchX { get; set; }
            public int SearchY { get; set; }
            public int FirstServerX { get; set; }
            public int FirstServerY { get; set; }
            public int JoinX { get; set; }
            public int JoinY { get; set; }
            public int RefreshX { get; set; }
            public int RefreshY { get; set; }
            public int BackX { get; set; }
            public int BackY { get; set; }
            public int CancelX { get; set; }
            public int CancelY { get; set; }
            public int RedOkayX { get; set; }
            public int RedOkayY { get; set; }
            public int ModJoinX { get; set; }
            public int ModJoinY { get; set; }
        }
    }
}
