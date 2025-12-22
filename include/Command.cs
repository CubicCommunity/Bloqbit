using Discord;
using Discord.WebSocket;

namespace Bloqbit.Include
{
    public abstract class Command
    {
        /// <summary>
        /// Discord slash command data 
        /// </summary>
        public abstract SlashCommandBuilder Builder { get; }

        /// <summary>
        /// Handler function for this command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public abstract Task ExecuteAsync(SocketSlashCommand command, DiscordSocketClient client);
    }
}