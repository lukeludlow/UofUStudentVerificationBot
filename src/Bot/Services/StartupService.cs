using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace UofUStudentVerificationBot
{
    public class StartupService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly DiscordSocketClient discordClient;
        private readonly CommandService commandService;
        private readonly IConfiguration config;

        public StartupService(IServiceProvider serviceProvider, DiscordSocketClient discordClient, CommandService commandService, IConfiguration config)
        {
            this.serviceProvider = serviceProvider;
            this.discordClient = discordClient;
            this.commandService = commandService;
            this.config = config;
        }

        public async Task StartAsync()
        {
            string discordToken = config["Discord:BotToken"];
            await discordClient.LoginAsync(TokenType.Bot, discordToken);
            await discordClient.StartAsync();
            await commandService.AddModulesAsync(Assembly.GetEntryAssembly(), serviceProvider);  
        }
        
    }
}