using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using ArkBotFramework.Infrastructure;

namespace ArkBotFramework.ArkWindow
{
    /// <summary>
    /// Handles game crash detection and recovery.
    /// Detects crashes, terminates the game process, and relaunches via Steam.
    /// </summary>
    public class CrashHandler
    {
        private readonly string _appId;
        private readonly string _windowTitle;
        private readonly Action<string> _logCritical;
        private IntPtr _hwnd;

        public CrashHandler(string appId, string windowTitle, Action<string> logCritical = null)
        {
            _appId = appId;
            _windowTitle = windowTitle;
            _logCritical = logCritical ?? Console.WriteLine;
        }

        /// <summary>
        /// Detects if the game has crashed by checking for crash dialog windows.
        /// </summary>
        public bool DetectCrash()
        {
            var processes = Process.GetProcesses();
            foreach (var process in processes)
            {
                try
                {
                    var title = process.MainWindowTitle;
                    if (title == "The UE-ShooterGame Game has crashed and will close" || 
                        title == "Crash!")
                    {
                        _logCritical("GAME HAS CRASHED");
                        return true;
                    }
                }
                catch
                {
                    // Ignore processes we can't access
                }
            }
            return false;
        }

        /// <summary>
        /// Closes the game by terminating its process.
        /// </summary>
        public void CloseGame(IntPtr hwnd)
        {
            try
            {
                var process = GetProcessByWindowHandle(hwnd);
                if (process != null)
                {
                    process.Kill();
                    process.WaitForExit(5000);
                    _logCritical($"Game with PID {process.Id} terminated");
                }
                else
                {
                    _logCritical("Process not found");
                }
            }
            catch (UnauthorizedAccessException)
            {
                _logCritical("No permissions to terminate");
            }
            catch (Exception e)
            {
                _logCritical($"Error: {e.Message}");
            }
        }

        /// <summary>
        /// Launches the game via Steam using the steam:// protocol.
        /// </summary>
        public void LaunchGameWithSteam()
        {
            string steamPath = FindSteamExecutable();
            
            if (!string.IsNullOrEmpty(steamPath) && System.IO.File.Exists(steamPath))
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = steamPath,
                    Arguments = $"steam://run/{_appId}",
                    UseShellExecute = true
                };
                Process.Start(startInfo);
                _logCritical($"Launching game with AppID {_appId} via Steam");
            }
            else
            {
                _logCritical("Steam executable not found at the expected location, cannot relaunch game");
            }
        }

        /// <summary>
        /// Reopens the game: closes current instance, waits, and launches via Steam.
        /// </summary>
        public IntPtr ReopenGame(IntPtr currentHwnd, TemplateMatching templateMatching)
        {
            CloseGame(currentHwnd);
            Thread.Sleep(10000);
            LaunchGameWithSteam();
            
            // Wait for join_last_session screen (up to 60 seconds)
            WaitForTemplate(templateMatching, "join_last_session", 0.7, 60);
            
            // Get new window handle after relaunch
            return WindowManager.FindWindowByTitle(_windowTitle);
        }

        /// <summary>
        /// Detects crash and performs full recovery if detected.
        /// </summary>
        public IntPtr? CrashRejoin(IntPtr currentHwnd, TemplateMatching templateMatching)
        {
            if (DetectCrash())
            {
                return ReopenGame(currentHwnd, templateMatching);
            }
            return null;
        }

        /// <summary>
        /// Finds the Steam executable by searching common process locations.
        /// </summary>
        private string FindSteamExecutable()
        {
            try
            {
                var steamProcess = Process.GetProcessesByName("steam").FirstOrDefault();
                if (steamProcess != null)
                {
                    return steamProcess.MainModule?.FileName;
                }

                // Fallback to common installation paths
                string[] commonPaths = new[]
                {
                    @"C:\Program Files (x86)\Steam\steam.exe",
                    @"C:\Program Files\Steam\steam.exe"
                };

                return commonPaths.FirstOrDefault(System.IO.File.Exists);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a process by its window handle.
        /// </summary>
        private Process GetProcessByWindowHandle(IntPtr hwnd)
        {
            GetWindowThreadProcessId(hwnd, out uint processId);
            
            try
            {
                return Process.GetProcessById((int)processId);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Helper method to wait for a template to appear.
        /// </summary>
        private void WaitForTemplate(TemplateMatching templateMatching, string template, double threshold, int timeoutSeconds)
        {
            var startTime = DateTime.Now;
            while ((DateTime.Now - startTime).TotalSeconds < timeoutSeconds)
            {
                if (templateMatching.CheckTemplateNoBounds(template, threshold))
                {
                    return;
                }
                Thread.Sleep(1000);
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
    }
}
