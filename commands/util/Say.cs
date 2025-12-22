using Bloqbit.Include;

using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace Bloqbit.Commands.Util
{
    public class SayCommand : Command
    {
        public override SlashCommandBuilder Builder => new SlashCommandBuilder()
        .WithName("say")
        .WithDescription("Send a message in a channel.")
        .WithIntegrationTypes([ApplicationIntegrationType.GuildInstall])
        .WithContextTypes([InteractionContextType.Guild])
        .WithNsfw(false)
        .AddOption(
            new SlashCommandOptionBuilder()
            .WithName("message")
            .WithDescription("The message to send.")
            .WithType(ApplicationCommandOptionType.String)
            .WithRequired(true)
        )
        .AddOption(
            new SlashCommandOptionBuilder()
            .WithName("channel")
            .WithDescription("A channel to send a message in.")
            .WithType(ApplicationCommandOptionType.Channel)
            .WithRequired(false)
            .AddChannelType(ChannelType.Text)
            .AddChannelType(ChannelType.Voice)
            .AddChannelType(ChannelType.News)
            .AddChannelType(ChannelType.Stage)
            .AddChannelType(ChannelType.NewsThread)
            .AddChannelType(ChannelType.PublicThread)
            .AddChannelType(ChannelType.PrivateThread)
        )
        .WithDefaultMemberPermissions(GuildPermission.ManageMessages);

        public override async Task ExecuteAsync(SocketSlashCommand command, DiscordSocketClient client)
        {
            var messageOpt = command.Data.Options.FirstOrDefault((o) => o.Name == "message");
            var channelOpt = command.Data.Options.FirstOrDefault((o) => o.Name == "channel");

            string message = messageOpt?.Value?.ToString() ?? "No message provided";
            SocketGuildChannel? channel = channelOpt?.Value as SocketGuildChannel;

            Embed respondEmbed = new EmbedBuilder()
            .WithDescription(message)
            .WithColor(Assets.Colors.Primary)
            .Build();

            if (channel is not null && channel.Guild is not null)
            {
                Log.Debug($"Channel of ID {channel.Id} is in a guild");

                RestUserMessage? sent = null;

                switch (channel.ChannelType)
                {
                    case ChannelType.Text:
                        if (channel is SocketTextChannel stc) sent = await stc.SendMessageAsync(embeds: [respondEmbed]);
                        Log.Info($"Custom message sent to text channel of ID {channel.Id}");
                        break;

                    case ChannelType.Voice:
                        if (channel is SocketVoiceChannel svc) sent = await svc.SendMessageAsync(embeds: [respondEmbed]);
                        Log.Info($"Custom message sent to voice channel of ID {channel.Id}");
                        break;

                    case ChannelType.News:
                        if (channel is SocketNewsChannel snc) sent = await snc.SendMessageAsync(embeds: [respondEmbed]);
                        Log.Info($"Custom message sent to news channel of ID {channel.Id}");
                        break;

                    case ChannelType.Stage:
                        if (channel is SocketStageChannel ssc) sent = await ssc.SendMessageAsync(embeds: [respondEmbed]);
                        Log.Info($"Custom message sent to stage channel of ID {channel.Id}");
                        break;

                    default:
                        sent = await command.Channel.SendMessageAsync(embeds: [respondEmbed]);
                        Log.Warn($"Channel of ID {channel.Id} was not valid type to send to");
                        break;
                }

                if (channel.Id == command.ChannelId || sent is null)
                {
                    await command.RespondAsync(text: $"{Assets.Icons.Check} Message sent.", flags: MessageFlags.Ephemeral);
                }
                else if (sent is not null)
                {
                    await command.RespondAsync(text: $"{Assets.Icons.Check} Message sent to {sent.GetJumpUrl()}.");
                }
            }
            else
            {
                await command.RespondAsync(embeds: [respondEmbed]);
            }
        }
    }
}