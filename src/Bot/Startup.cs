using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Discord.WebSocket;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using Discord;
using System.Reflection;

namespace UofUStudentVerificationBot
{
    public class Startup
    {

        public static async Task MainAsync()
        {
            Startup startup = new Startup();
            await startup.StartAsync();
        }

        public async Task StartAsync()
        {
            IServiceProvider serviceProvider = BuildServices();
            await StartRunningBot(serviceProvider);
            await Task.Delay(-1); // keep the bot alive until the program is closed
        }

        private async Task StartRunningBot(IServiceProvider serviceProvider)
        {
            // start running some of the required services that we need
            serviceProvider.GetRequiredService<CommandHandler>();
            serviceProvider.GetRequiredService<LogService>();
            DiscordSocketClient discordClient = serviceProvider.GetRequiredService<DiscordSocketClient>();
            CommandService commandService = serviceProvider.GetRequiredService<CommandService>();
            IConfiguration config = serviceProvider.GetRequiredService<IConfiguration>();
            await discordClient.LoginAsync(TokenType.Bot, config["Discord:BotToken"]);
            await discordClient.StartAsync();
            await commandService.AddModulesAsync(Assembly.GetEntryAssembly(), serviceProvider); 
        }

        private IServiceProvider BuildServices()
        {
            IConfiguration config = BuildConfig();
            IServiceCollection serviceCollection = new ServiceCollection()
                .AddSingleton<LogService>()
                .AddSingleton<CommandHandler>()
                .AddSingleton<StudentVerificationService>()
                .AddSingleton<StudentRepository>()
                .AddSingleton<EmailService>()
                .AddSingleton<RoleAssignmentService>()
                .AddDbContext<StudentDbContext>(options => options.UseSqlite($"DataSource={config["Db:Path"]}"))
                .AddSingleton(new DiscordSocketClient())
                .AddSingleton(new CommandService())
                .AddSingleton(config);
            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            return serviceProvider;
        }

        private IConfiguration BuildConfig()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();
            return builder.Build();
        }

    }
}