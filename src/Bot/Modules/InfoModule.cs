using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace UofUStudentVerificationBot
{
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient discordClient;
        private readonly CommandService commandService;

        public InfoModule(DiscordSocketClient discordClient, CommandService commandService)
        {
            this.discordClient = discordClient;
            this.commandService = commandService;
        }

        // [Command("info")]
        // [Summary("get more information about this bot")]
        // public async Task Info()
        // {
        //     // TODO
        //     // framework (dotnet core / netcoreapp, discord.net)
        //     // author (luke)
        //     // source code github link
        //     // deployed to azure secure vm
        //     // use "help" to see all available commands
        //     // create an embed so i can do a hyperlink to github and stuff
        //     string infoMessage = "todo help message. made by luke. here's the github link.";
        //     await Context.Channel.SendMessageAsync(infoMessage);
        // }

        // [Command("help")]
        // [Summary("show help messages for all available commands")]
        // public async Task Help()
        // {
        //     // TODO
        //     string helpMessage = "todo help message";
        //     await Context.Channel.SendMessageAsync(helpMessage);
        // }

        // TODO create a default fallthrough command that executes anytime the bot is directly mentioned 
        // [Command("")]
        // public async Task DefaultCommand()
        // {
        //     if (Context.Message.HasMentionPrefix(discordClient.CurrentUser, ref argPos)) {
        //         await Help();
        //     }
        // }

    }
}