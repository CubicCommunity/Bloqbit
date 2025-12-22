using Bloqbit.Include;

using Discord;
using Discord.WebSocket;

namespace Bloqbit.Commands.Moderator
{
    public class BanCommand : Command
    {
        public override SlashCommandBuilder Builder => new SlashCommandBuilder()
        .WithName("ban")
        .WithDescription("Ban a member from the server.")
        .WithIntegrationTypes([ApplicationIntegrationType.GuildInstall])
        .WithContextTypes([InteractionContextType.Guild])
        .WithNsfw(false)
        .AddOption(
            new SlashCommandOptionBuilder()
            .WithName("user")
            .WithDescription("The member to ban.")
            .WithType(ApplicationCommandOptionType.User)
            .WithRequired(true)
        )
        .AddOption(
            new SlashCommandOptionBuilder()
            .WithName("reason")
            .WithDescription("The reason to ban the member.")
            .WithType(ApplicationCommandOptionType.String)
            .WithRequired(false)
        )
        .WithDefaultMemberPermissions(GuildPermission.BanMembers);

        public override async Task ExecuteAsync(SocketSlashCommand command, DiscordSocketClient client)
        {
            var userOpt = command.Data.Options.FirstOrDefault((o) => o.Name == "user");
            var reasonOpt = command.Data.Options.FirstOrDefault((o) => o.Name == "reason");

            SocketGuildUser? user = userOpt?.Value as SocketGuildUser;
            string reason = reasonOpt?.Value?.ToString() ?? "Unspecified";

            if (user is not null)
            {
                Log.Debug("[i] Banning user from guild via command");

                if (user.GuildPermissions.BanMembers)
                {
                    await command.RespondAsync(text: $"{Assets.Icons.XMark} You cannot moderate another moderator.", flags: MessageFlags.Ephemeral);
                    Log.Error($"[x] Attempted to ban user with moderator permission");
                    return;
                }
                else
                {
                    Log.Debug("[ii] Target user does not have moderator permission");

                    if (command.GuildId.HasValue)
                    {
                        Log.Debug("[iii] Ban command invoked in a guild context");

                        var guild = client.GetGuild(command.GuildId.Value);

                        if (guild is not null)
                        {
                            Log.Debug("[iv] Found guild to process ban");

                            try
                            {
                                Log.Debug("[v] Processing ban for user from guild");

                                await guild.BanUserAsync(user, 7 * 86400, new RequestOptions()
                                {
                                    AuditLogReason = $"{command.User.Username} | Ban - {reason}",
                                    RetryMode = RetryMode.AlwaysRetry
                                });

                                Embed respondEmbed = new EmbedBuilder()
                                .WithTitle($"{Assets.Icons.NoEntry} User Banned")
                                .WithColor(Assets.Colors.Primary)
                                .AddField(
                                    new EmbedFieldBuilder()
                                    .WithName("User")
                                    .WithValue($"**{user.Username}**")
                                    .WithIsInline(false)
                                )
                                .AddField(
                                    new EmbedFieldBuilder()
                                    .WithName("Moderator")
                                    .WithValue($"**{command.User.Username}**")
                                    .WithIsInline(false)
                                )
                                .AddField(
                                    new EmbedFieldBuilder()
                                    .WithName("Reason")
                                    .WithValue($"{reason}")
                                    .WithIsInline(false)
                                )
                                .Build();

                                await command.RespondAsync(embeds: [respondEmbed]);

                                try
                                {
                                    Log.Debug("[vi] Sending DM to banned user");

                                    var dmEmbed = new EmbedBuilder()
                                    .WithTitle($"{Assets.Icons.NoEntry} Banned")
                                    .WithDescription($"You were __banned__ from **{guild.Name}**.")
                                    .WithColor(Assets.Colors.Primary)
                                    .AddField(
                                        new EmbedFieldBuilder()
                                        .WithName("Reason")
                                        .WithValue($"{reason}")
                                        .WithIsInline(false)
                                    )
                                    .AddField(
                                        new EmbedFieldBuilder()
                                        .WithName("Reviewed")
                                        .WithValue($"<t:{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}:F>")
                                        .WithIsInline(false)
                                    )
                                    .Build();

                                    await user.SendMessageAsync(embeds: [dmEmbed]);
                                }
                                catch (Exception e)
                                {
                                    Log.Error($"{e.Message}");
                                }

                                Log.Info("[o] Banned user from guild");
                            }
                            catch (Exception e)
                            {
                                await command.RespondAsync(text: $"{Assets.Icons.XMark} An error occurred.", flags: MessageFlags.Ephemeral);
                                Log.Error(e.Message);
                            }
                        }
                        else
                        {
                            await command.RespondAsync(text: $"{Assets.Icons.XMark} An error occurred.", flags: MessageFlags.Ephemeral);
                            Log.Error("Ban command invoked but guild could not be found");
                        }
                    }
                    else
                    {
                        await command.RespondAsync(text: $"{Assets.Icons.XMark} An error occurred.", flags: MessageFlags.Ephemeral);
                        Log.Error("Ban command invoked outside of a guild context");
                    }
                }
            }
            else
            {
                await command.RespondAsync(text: $"{Assets.Icons.XMark} Invalid user.", flags: MessageFlags.Ephemeral);
                Log.Error("Ban command invoked with invalid user");
            }
        }
    }
}