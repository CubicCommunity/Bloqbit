using System.Runtime.InteropServices;

using Bloqbit.Include;

using Discord;
using Discord.WebSocket;

namespace Bloqbit.Commands.Util
{
    public class AboutCommand : Command
    {
        public override SlashCommandBuilder Builder => new SlashCommandBuilder()
        .WithName("about")
        .WithDescription("View detailed information about the current installation of Bloqbit .")
        .WithIntegrationTypes([ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall])
        .WithContextTypes([InteractionContextType.Guild, InteractionContextType.PrivateChannel, InteractionContextType.BotDm])
        .WithNsfw(false);

        public override async Task ExecuteAsync(SocketSlashCommand command, DiscordSocketClient client)
        {
            Log.Info($"Sending about message in guild of ID {command.GuildId}");

            var version = Include.Version.Get();
            var uptimeSince = Math.Floor(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - Uptime.Get().TotalSeconds);

            Embed respondEmbed = new EmbedBuilder()
            .WithAuthor(
                new EmbedAuthorBuilder()
                .WithName(client.CurrentUser.Username)
                .WithIconUrl(client.CurrentUser.GetAvatarUrl(ImageFormat.Auto, 512))
            )
            .WithTitle($"Bloqbit `v{version}`")
            .WithDescription($"Running as Discord bot client **`{client.CurrentUser.Username}`**`#{client.CurrentUser.Discriminator}` (`{client.CurrentUser.Id}`) on shard **#{client.ShardId}**")
            .WithColor(Assets.Colors.Primary)
            .AddField(
                new EmbedFieldBuilder()
                .WithName(".NET Version")
                .WithValue($"`{RuntimeInformation.FrameworkDescription}`")
                .WithIsInline(true)
            )
            .AddField(
                new EmbedFieldBuilder()
                .WithName("Uptime")
                .WithValue($"{Uptime.GetFormatted()} • <t:{uptimeSince}:R>")
                .WithIsInline(true)
            )
            .AddField(
                new EmbedFieldBuilder()
                .WithName($"{Assets.Icons.Self} Need Help?")
                .WithValue("Join **[dsc.gg/bloqbit](https://www.dsc.gg/bloqbit)**")
                .WithIsInline(false)
            )
            .WithFooter(
                new EmbedFooterBuilder()
                .WithText("This command is in the works - expect more information to be added soon.")
            )
            .Build();

            await command.RespondAsync(embeds: [respondEmbed]);
        }
    }
}