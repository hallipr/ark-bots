using System;
using System.Threading;

using ArkBotFramework.Infrastructure;

namespace ArkBotFramework.ArkWindow
{
    /// <summary>
    /// Orchestrates the full reconnection process.
    /// Detects disconnections and navigates through menus to rejoin the server.
    /// </summary>
    public class ReconnectSystem
    {
        private readonly CrashHandler _crashHandler;
        private readonly MainMenu _mainMenu;
        private readonly MultiplayerMenu _multiplayerMenu;
        private readonly TemplateMatching _templateMatching;
        private readonly string _serverName;
        private readonly Action<string> _logCritical;

        public ReconnectSystem(
            CrashHandler crashHandler,
            MainMenu mainMenu,
            MultiplayerMenu multiplayerMenu,
            TemplateMatching templateMatching,
            string serverName,
            Action<string> logCritical = null)
        {
            _crashHandler = crashHandler;
            _mainMenu = mainMenu;
            _multiplayerMenu = multiplayerMenu;
            _templateMatching = templateMatching;
            _serverName = serverName;
            _logCritical = logCritical ?? Console.WriteLine;
        }

        /// <summary>
        /// Check if the client is disconnected.
        /// </summary>
        public bool CheckDisconnected()
        {
            return _templateMatching.CheckTemplateNoBounds("escape", 0.7);
        }

        /// <summary>
        /// Performs full rejoin sequence: restart game, navigate menus, and join server.
        /// </summary>
        public void RejoinServer(IntPtr currentHwnd)
        {
            bool joined = false;
            var startTime = DateTime.Now;

            // Reopen game first
            var newHwnd = _crashHandler.ReopenGame(currentHwnd, _templateMatching);

            while (!joined)
            {
                // If more than 5 minutes have passed, restart the game
                if ((DateTime.Now - startTime).TotalMinutes >= 5)
                {
                    _logCritical("Time was greater than 5 minutes, restarting game now");
                    newHwnd = _crashHandler.ReopenGame(newHwnd, _templateMatching);
                    startTime = DateTime.Now;
                }

                // Navigate through menus
                _mainMenu.EnterMenu();
                Thread.Sleep(500);

                // Join multiplayer
                _multiplayerMenu.JoinServer(_serverName);
                Thread.Sleep(1000);

                // Check if we successfully joined
                if (_templateMatching.CheckTemplateNoBounds("tribelog_check", 0.8) ||
                    _templateMatching.CheckTemplate("death_regions", 0.7))
                {
                    joined = true;
                    _logCritical("Successfully rejoined server");
                    return;
                }

                // Wait before retrying
                Thread.Sleep(2000);
            }
        }
    }
}
