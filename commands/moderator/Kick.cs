using Bloqbit.Include;

using Discord;
using Discord.WebSocket;

namespace Bloqbit.Commands.Moderator
{
    public class KickCommand : Command
    {
        public override SlashCommandBuilder Builder => new SlashCommandBuilder()
        .WithName("kick")
        .WithDescription("Kick a member from the server.")
        .WithIntegrationTypes([ApplicationIntegrationType.GuildInstall])
        .WithContextTypes([InteractionContextType.Guild])
        .WithNsfw(false)
        .AddOption(
            new SlashCommandOptionBuilder()
            .WithName("user")
            .WithDescription("The member to kick.")
            .WithType(ApplicationCommandOptionType.User)
            .WithRequired(true)
        )
        .AddOption(
            new SlashCommandOptionBuilder()
            .WithName("reason")
            .WithDescription("The reason to kick the member.")
            .WithType(ApplicationCommandOptionType.String)
            .WithRequired(false)
        )
        .WithDefaultMemberPermissions(GuildPermission.KickMembers);

        public override async Task ExecuteAsync(SocketSlashCommand command, DiscordSocketClient client)
        {
            var userOpt = command.Data.Options.FirstOrDefault((o) => o.Name == "user");
            var reasonOpt = command.Data.Options.FirstOrDefault((o) => o.Name == "reason");

            SocketGuildUser? user = userOpt?.Value as SocketGuildUser;
            string reason = reasonOpt?.Value?.ToString() ?? "Unspecified";

            if (user is not null)
            {
                Log.Debug("[i] Kicking user from guild via command");

                if (user.GuildPermissions.KickMembers)
                {
                    await command.RespondAsync(text: $"{Assets.Icons.XMark} You cannot moderate another moderator.", flags: MessageFlags.Ephemeral);
                    Log.Error($"[x] Attempted to kick user with moderator permission");
                    return;
                }
                else
                {
                    Log.Debug("[ii] Target user does not have moderator permission");

                    if (command.GuildId.HasValue)
                    {
                        Log.Debug("[iii] Kick command invoked in a guild context");

                        var guild = client.GetGuild(command.GuildId.Value);

                        if (guild is not null)
                        {
                            Log.Debug("[iv] Guild found, proceeding to kick user");

                            await user.KickAsync(reason: reason);

                            await command.RespondAsync(text: $"{Assets.Icons.Check} Successfully kicked {user.Mention}.\n**Reason:** {reason}", flags: MessageFlags.Ephemeral);
                            Log.Info($"[o] Kicked user {user.Username} ({user.Id}) from guild {guild.Name} ({guild.Id})");
                        }
                        else
                        {
                            await command.RespondAsync(text: $"{Assets.Icons.XMark} Guild not found.", flags: MessageFlags.Ephemeral);
                            Log.Error($"[x] Guild with ID {command.GuildId.Value} not found");
                        }
                    }
                    else
                    {
                        await command.RespondAsync(text: $"{Assets.Icons.XMark} This command can only be used in a server.", flags: MessageFlags.Ephemeral);
                        Log.Error($"[x] Kick command invoked outside of guild context");
                    }
                }
            }
            else
            {
                await command.RespondAsync(text: $"{Assets.Icons.XMark} User not found.", flags: MessageFlags.Ephemeral);
                Log.Error($"[x] User to kick not found or invalid");
            }
        }
    }
}