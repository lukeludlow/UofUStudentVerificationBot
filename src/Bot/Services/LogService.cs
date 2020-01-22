using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace UofUStudentVerificationBot
{
    public class LogService
    {
        private readonly DiscordSocketClient discordClient;
        private readonly CommandService commandService;

        public LogService(DiscordSocketClient discordClient, CommandService commandService)
        {
            this.discordClient = discordClient;
            this.commandService = commandService;
            this.discordClient.Log += LogAsync;
            this.commandService.Log += LogAsync;
        }

        public async Task LogAsync(LogMessage message)
        {
            await Console.Out.WriteLineAsync(message.ToString());
        }

    }
}