using Bloqbit.Include;

using Discord;
using Discord.WebSocket;

namespace Bloqbit.Commands.Util
{
    public class MemberCountCommand : Command
    {
        public override SlashCommandBuilder Builder => new SlashCommandBuilder()
        .WithName("member-count")
        .WithDescription("Ping the bot, test its latency.")
        .WithIntegrationTypes([ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall])
        .WithContextTypes([InteractionContextType.Guild, InteractionContextType.PrivateChannel, InteractionContextType.BotDm])
        .WithNsfw(false);

        public override async Task ExecuteAsync(SocketSlashCommand command, DiscordSocketClient client)
        {
            if (command.GuildId.HasValue)
            {
                var guild = client.GetGuild(command.GuildId.Value);

                if (guild is not null)
                {
                    Log.Debug($"Getting member and bot counts for guild of ID ${command.GuildId}");

                    int total = guild.MemberCount;
                    int bots = guild.Users.Count((u) => u.IsBot);

                    Embed respondEmbed = new EmbedBuilder()
                    .WithAuthor(
                        new EmbedAuthorBuilder()
                        .WithName(guild.Name)
                        .WithIconUrl(guild.IconUrl)
                    )
                    .WithColor(Assets.Colors.Primary)
                    .AddField(
                        new EmbedFieldBuilder()
                        .WithName("Member Count")
                        .WithValue($"**```{total}```**")
                        .WithIsInline(false)
                    )
                    .WithFooter(
                        new EmbedFooterBuilder()
                        .WithText($"{bots} Bots")
                    )
                    .Build();

                    Log.Info($"Guild of ID {command.GuildId} has {total} members ({bots} bots)");

                    await command.RespondAsync(embeds: [respondEmbed]);
                }
                else
                {
                    await command.RespondAsync(text: $"{Assets.Icons.XMark} An error occurred.", flags: MessageFlags.Ephemeral);
                    Log.Error("Member count command invoked but guild could not be found");
                }
            }
            else
            {
                await command.RespondAsync(text: $"{Assets.Icons.XMark} An error occurred.", flags: MessageFlags.Ephemeral);
                Log.Error("Member count command invoked outside of a guild context");
            }
        }
    }
}