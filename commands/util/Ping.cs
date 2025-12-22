using Bloqbit.Include;

using Discord;
using Discord.WebSocket;

namespace Bloqbit.Commands.Util
{
    public class PingCommand : Command
    {
        public override SlashCommandBuilder Builder => new SlashCommandBuilder()
        .WithName("ping")
        .WithDescription("Ping the bot, test its latency.")
        .WithIntegrationTypes([ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall])
        .WithContextTypes([InteractionContextType.Guild, InteractionContextType.PrivateChannel, InteractionContextType.BotDm])
        .WithNsfw(false);

        public override async Task ExecuteAsync(SocketSlashCommand command, DiscordSocketClient client)
        {
            var now = DateTimeOffset.UtcNow;
            var cmdTime = command.CreatedAt;

            double ping = Math.Floor((now - cmdTime).TotalMilliseconds);
            Log.Info($"Pinging command in guild with {ping}ms");

            Embed respondEmbed = new EmbedBuilder()
            .WithTitle($"{Assets.Icons.Info} Ping")
            .WithColor(Assets.Colors.Primary)
            .AddField(
                new EmbedFieldBuilder()
                .WithName("Ping")
                .WithValue($"{ping}ms")
                .WithIsInline(false)
            )
            .AddField(
                new EmbedFieldBuilder()
                .WithName("API Ping")
                .WithValue($"{Math.Round((double)client.Latency)}ms")
                .WithIsInline(false)
            )
            .Build();

            await command.RespondAsync(embeds: [respondEmbed], flags: MessageFlags.Ephemeral);
        }
    }
}