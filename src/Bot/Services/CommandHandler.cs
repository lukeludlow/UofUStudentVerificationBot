using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace UofUStudentVerificationBot
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient discordClient;
        private readonly CommandService commandService;
        private readonly LogService logService;
        private readonly IConfiguration config;
        private readonly IServiceProvider serviceProvider;

        public CommandHandler(DiscordSocketClient discordClient, CommandService commandService, LogService logService, IConfiguration config, IServiceProvider services)
        {
            this.discordClient = discordClient;
            this.commandService = commandService;
            this.logService = logService;
            this.config = config;
            this.serviceProvider = services;
            this.discordClient.MessageReceived += OnMessageReceivedAsync;
            this.commandService.CommandExecuted += HandlePostExecution;
        }

        private async Task OnMessageReceivedAsync(SocketMessage messageParam)
        {
            SocketUserMessage message = messageParam as SocketUserMessage;
            // ensure the message is from a user
            if (message == null) return;
            // ignore self when checking commands
            if (message.Author.Id == discordClient.CurrentUser.Id) return;
            // if the message is prefixed with "$" or directly mentions the bot, then go ahead and execute the command
            int argPos = 0;
            if (message.Channel.GetType() == typeof(SocketDMChannel)) {
                await logService.LogAsync(new LogMessage(LogSeverity.Info, "CommandHandler", $"received DM"));
            }
            if (message.HasCharPrefix('$', ref argPos) || message.HasMentionPrefix(discordClient.CurrentUser, ref argPos)) {
                SocketCommandContext context = new SocketCommandContext(discordClient, message);
                await commandService.ExecuteAsync(context, argPos, serviceProvider);
            }
        }

        // private async Task OnCommandExecutedAsync(Optional<CommandInfo> commandInfo, ICommandContext context, IResult result)
        // {
        //     string commandName = commandInfo.IsSpecified ? commandInfo.Value.Name : "unknown command";
        //     await logService.LogAsync(new LogMessage(LogSeverity.Info, "CommandHandler", $"executed command \"{commandName}\""));
        // }

        public async Task HandlePostExecution(Optional<CommandInfo> commandInfo, ICommandContext context, IResult result)
        {
            string commandName = commandInfo.IsSpecified ? commandInfo.Value.Name : "unknown command";
            string resultString = result.ErrorReason ?? "success";
            await logService.LogInfo("PostExecutionService", $"executed command: {commandName}, result: {resultString}");
        }

    }
}