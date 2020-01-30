using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace UofUStudentVerificationBot
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient discordClient;
        private readonly CommandService commandService;
        private readonly LogService logService;
        private readonly StudentVerificationService verificationService;
        private readonly IConfiguration config;
        private readonly IServiceProvider serviceProvider;

        public CommandHandler(IServiceProvider serviceProvider)
        {
            this.discordClient = serviceProvider.GetRequiredService<DiscordSocketClient>();
            this.commandService = serviceProvider.GetRequiredService<CommandService>();
            this.logService = serviceProvider.GetRequiredService<LogService>();
            this.verificationService = serviceProvider.GetRequiredService<StudentVerificationService>();
            this.config = serviceProvider.GetRequiredService<IConfiguration>();
            this.serviceProvider = serviceProvider;
            this.discordClient.MessageReceived += OnMessageReceivedAsync;
            this.commandService.CommandExecuted += HandlePostExecution;
        }

        private async Task OnMessageReceivedAsync(SocketMessage messageParam)
        {
            SocketUserMessage message = messageParam as SocketUserMessage;
            await logService.LogInfo("CommandHandler", $"received message. channel: {message.Channel.Name}, content: {message.Content}");
            // ensure the message is from a user
            if (message == null) return;
            // ignore self when checking commands
            if (message.Author.Id == discordClient.CurrentUser.Id) return;
            // check if the user DMed the bot
            if (IsDirectMessage(message) && !IsVerifyOrResetCommand(message)) {
                await logService.LogInfo("CommandHandler", "receiving direct message verification code");
                await ReceiveDirectMessageVerificationCode(message);
                return;
            }
            if (IsDirectMessage(message) || IsMessageInVerificationChannel(message)) {
                int argPos = 0;
                if (message.HasCharPrefix('$', ref argPos) || message.HasMentionPrefix(discordClient.CurrentUser, ref argPos)) {
                    await logService.LogInfo("CommandHandler", "handling command with verification module");
                    SocketCommandContext context = new SocketCommandContext(discordClient, message);
                    await commandService.ExecuteAsync(context, argPos, serviceProvider);
                    return;
                }
            }
            await logService.LogInfo("CommandHandler", "the message did not match any available commands, no action will be taken");
        }

        private bool IsVerifyOrResetCommand(SocketUserMessage message)
        {
            return message.Content.Contains("$verify") || message.Content.Contains("$reset");
            // int argPos = 0;
            // if 
            // bool hasCommandPrefix = message.HasCharPrefix('$', ref argPos);
            // bool isVerifyCommand = message.HasStringPrefix("$verify", ref argPos);
            // bool isResetCommand = message.HasStringPrefix("$reset", ref argPos);
            // return (isVerifyCommand || isResetCommand);
        }


        private bool IsDirectMessage(SocketUserMessage message)
        {
            return message.Channel.GetType() == typeof(SocketDMChannel);
        }

        // the bot is only allowed to listen/talk in the specified verification channel
        private bool IsMessageInVerificationChannel(SocketUserMessage message)
        {
            return message.Channel.Id == config.GetValue<ulong>("Discord:VerificationChannelID");
        }


        // TODO this is super janky please fix this
        private async Task ReceiveDirectMessageVerificationCode(SocketUserMessage message)
        {
            VerificationResult completeVerificationResult = verificationService.CompleteVerification(message.Author.Id, message.Content);
            if (completeVerificationResult.IsSuccess) {
                await message.Channel.SendMessageAsync(completeVerificationResult.Reason);
            } else {
                await message.Channel.SendMessageAsync($"error: {completeVerificationResult.Reason}");
            }
        }

        public async Task HandlePostExecution(Optional<CommandInfo> commandInfo, ICommandContext context, IResult result)
        {
            switch (result) {
                case VerificationResult verificationResult:
                    if (verificationResult.IsSuccess) {
                        await logService.LogInfo("CommandHandler", $"verification result: {verificationResult.Reason}");
                        await Discord.UserExtensions.SendMessageAsync(context.Message.Author, verificationResult.Reason);
                    } else {
                        await logService.LogError("CommandHandler", $"verification result: {verificationResult.Reason}");
                        await Discord.UserExtensions.SendMessageAsync(context.Message.Author, $"{verificationResult.Reason}");
                    }
                    break;
                default:
                    string commandName = commandInfo.IsSpecified ? commandInfo.Value.Name : "unknown command";
                    string resultString = result.IsSuccess ? "success" : result.ErrorReason;
                    await logService.LogInfo("PostExecutionService", $"executed command: {commandName}, result: {resultString}");
                    break;
            }
        }

        private async Task HandleVerificationResult(Optional<CommandInfo> commandInfo, ICommandContext context, VerificationResult verificationResult)
        {
            if (verificationResult.IsSuccess) {
                switch (commandInfo.Value.Name) {
                    case "verify":
                        await Discord.UserExtensions.SendMessageAsync(context.Message.Author, verificationResult.Reason);
                        break;
                    case "reset":
                        break;
                }
            } else {
                await logService.LogError("CommandHandler", $"verification command \"{commandInfo.Value.Name}\" failed: {verificationResult.Reason}");
            }
        }

    }
}