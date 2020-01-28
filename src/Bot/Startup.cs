using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Discord.WebSocket;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;

namespace UofUStudentVerificationBot
{
    public class Startup
    {

        public IConfiguration Config { get; private set; }

        public Startup()
        {
        }

        public static async Task MainAsync()
        {
            Startup startup = new Startup();
            await startup.RunAsync();
        }

        public async Task RunAsync()
        {
            this.Config = BuildConfig();

            IServiceCollection services = new ServiceCollection();
            services.AddSingleton<LogService>()
                    .AddSingleton<StartupService>()
                    .AddSingleton<CommandHandler>()
                    .AddSingleton<StudentVerificationService>()
                    .AddSingleton<IStudentRepository, StudentRepository>()
                    .AddSingleton<IEmailService, EmailService>()
                    .AddSingleton<IRoleAssignmentService, RoleAssignmentService>()
                    .AddDbContext<StudentDbContext>(options => options.UseSqlite("DataSource=students.db"))
                    .AddSingleton(new DiscordSocketClient())
                    .AddSingleton(new CommandService())
                    .AddSingleton(this.Config);

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetRequiredService<LogService>();
            serviceProvider.GetRequiredService<StartupService>();
            serviceProvider.GetRequiredService<CommandHandler>();
            // serviceProvider.GetRequiredService<StudentVerificationService>();

            // the startup service actually runs the bot
            await serviceProvider.GetRequiredService<StartupService>().StartAsync();
            // keep the bot alive until the program is closed
            await Task.Delay(-1);
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