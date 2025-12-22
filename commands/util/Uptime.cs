using Bloqbit.Include;

using Discord;
using Discord.WebSocket;

namespace Bloqbit.Commands.Util
{
    public class UptimeCommand : Command
    {
        public override SlashCommandBuilder Builder => new SlashCommandBuilder()
        .WithName("uptime")
        .WithDescription("Check how long the bot has been online.")
        .WithIntegrationTypes([ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall])
        .WithContextTypes([InteractionContextType.Guild, InteractionContextType.PrivateChannel, InteractionContextType.BotDm])
        .WithNsfw(false);

        public override async Task ExecuteAsync(SocketSlashCommand command, DiscordSocketClient client)
        {
            string formattedUptime = Uptime.GetFormatted();
            Log.Info($"Uptime command requested with current uptime of {formattedUptime}");

            Embed respondEmbed = new EmbedBuilder()
            .WithDescription($"{Assets.Icons.Info} Bloqbit has been running for `{formattedUptime}`!")
            .WithColor(Assets.Colors.Primary)
            .Build();

            await command.RespondAsync(embeds: [respondEmbed], flags: MessageFlags.Ephemeral);
        }
    }
}