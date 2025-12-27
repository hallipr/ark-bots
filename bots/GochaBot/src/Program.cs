using ArkBotFramework.ArkWindow;
using ArkBotFramework.Infrastructure;
using Microsoft.Extensions.Logging;

internal class Program
{
    private static int Main(string[] args)
    {
        Console.WriteLine("ARK Gacha Bot - Testing Window Handling");

        // Setup logging
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddConsole()
                .SetMinimumLevel(LogLevel.Debug);
        });

        var logger = loggerFactory.CreateLogger("GochaBot");

        try
        {
            // Find ARK window
            logger.LogInformation("Looking for ArkAscended window...");
            var windowManager = new WindowManager("ArkAscended", new System.Drawing.Rectangle(0, 0, 2560, 1440));
            logger.LogInformation("Found ARK window: {Hwnd}", windowManager.Hwnd);

            // Setup screen capture and template matching
            var screenCapture = new ScreenCapture(windowManager.Hwnd);
            logger.LogInformation("Screen resolution: {Resolution}", screenCapture.ScreenResolution);

            var templateMatching = new TemplateMatching(screenCapture, msg => logger.LogDebug("Template: {Message}", msg));

            // Setup main menu handler
            var mainMenu = new MainMenu(screenCapture, windowManager, templateMatching);

            // Check if we're at the main menu
            if (mainMenu.IsOpen())
            {
                logger.LogInformation("Main menu detected!");
                logger.LogInformation("Attempting to enter menu and navigate...");

                mainMenu.EnterMenu();

                logger.LogInformation("Menu navigation complete!");
            }
            else
            {
                logger.LogWarning("Main menu not detected. Make sure ARK is at the main menu.");
            }
            return 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during window handling test");
            return 1;
        }
    }
}