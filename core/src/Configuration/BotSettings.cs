using System;

namespace ArkBotFramework.Configuration
{
    /// <summary>
    /// Base configuration settings for the bot framework.
    /// Contains only framework-level settings, not bot-specific implementation details.
    /// </summary>
    public class BotSettings
    {
        /// <summary>
        /// Lag offset multiplier for timing adjustments.
        /// Default: 1.0 (no offset). Increase for slower systems/networks.
        /// </summary>
        public double LagOffset { get; set; } = 1.0;

        /// <summary>
        /// Command prefix for Discord bot commands.
        /// Default: "%"
        /// </summary>
        public string CommandPrefix { get; set; } = "%";

        /// <summary>
        /// Whether running in single-player mode.
        /// </summary>
        public bool Singleplayer { get; set; } = false;

        /// <summary>
        /// Server number/name for multiplayer reconnection.
        /// </summary>
        public string ServerNumber { get; set; } = "0";

        /// <summary>
        /// Discord channel ID for gacha logs.
        /// </summary>
        public ulong LogChannelGacha { get; set; } = 111111111111111;

        /// <summary>
        /// Discord channel ID for active queue logs.
        /// </summary>
        public ulong LogActiveQueue { get; set; } = 111111111111111;

        /// <summary>
        /// Discord channel ID for waiting queue logs.
        /// </summary>
        public ulong LogWaitQueue { get; set; } = 111111111111111;

        /// <summary>
        /// Discord bot API key.
        /// </summary>
        public string DiscordApiKey { get; set; } = "";

        /// <summary>
        /// Validates that required settings are configured.
        /// </summary>
        /// <returns>True if settings are valid, false otherwise</returns>
        public virtual bool Validate()
        {
            if (string.IsNullOrWhiteSpace(DiscordApiKey))
            {
                Console.WriteLine("Warning: Discord API key not configured");
                return false;
            }

            if (LagOffset <= 0)
            {
                Console.WriteLine("Warning: LagOffset must be positive");
                return false;
            }

            return true;
        }
    }
}
