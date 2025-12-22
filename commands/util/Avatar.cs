using Bloqbit.Include;

using Discord;
using Discord.WebSocket;

namespace Bloqbit.Commands.Util
{
    public class AvatarCommand : Command
    {
        public override SlashCommandBuilder Builder => new SlashCommandBuilder()
        .WithName("avatar")
        .WithDescription("Get the avatar of a user.")
        .WithIntegrationTypes([ApplicationIntegrationType.GuildInstall])
        .WithContextTypes([InteractionContextType.Guild])
        .WithNsfw(false)
        .AddOption(
            new SlashCommandOptionBuilder()
            .WithName("user")
            .WithDescription("The user to get the avatar of.")
            .WithType(ApplicationCommandOptionType.User)
            .WithRequired(false)
        )
        .WithDefaultMemberPermissions(GuildPermission.ViewChannel);

        public override async Task ExecuteAsync(SocketSlashCommand command, DiscordSocketClient client)
        {
            var userOpt = command.Data.Options.FirstOrDefault((o) => o.Name == "user");
            SocketGuildUser? user = userOpt?.Value as SocketGuildUser ?? command.User as SocketGuildUser;

            if (user is not null)
            {
                Embed respondEmbed = new EmbedBuilder()
                .WithTitle($"{Assets.Icons.Info} {user.Username}'s Avatar")
                .WithColor(Assets.Colors.Primary)
                .WithImageUrl(user.GetDisplayAvatarUrl(ImageFormat.Auto, 1024))
                .WithThumbnailUrl(user.GetAvatarUrl(ImageFormat.Auto, 1024))
                .Build();

                await command.RespondAsync(embeds: [respondEmbed]);
            }
            else
            {
                await command.RespondAsync(text: $"{Assets.Icons.XMark} Invalid user.", flags: MessageFlags.Ephemeral);
                Log.Error("Couldn't find user for avatar command");
            }
        }
    }
}