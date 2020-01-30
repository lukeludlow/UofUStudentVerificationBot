using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace UofUStudentVerificationBot
{
    public class VerificationModule : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient discordClient;
        private readonly CommandService commandService;
        private readonly StudentVerificationService verificationService;

        public VerificationModule(DiscordSocketClient discordClient, CommandService commandService, StudentVerificationService verificationService)
        {
            this.discordClient = discordClient;
            this.commandService = commandService;
            this.verificationService = verificationService;
        }

        [Command("verify")]
        [Summary("get verified")]
        public async Task<RuntimeResult> Verify(string uID) => await Task.Run(() =>
            verificationService.BeginVerification(Context.User.Id, uID)
        );

        [Command("reset")]
        [Summary("reset verification status (this will remove your verified student permissions and unlink your uID)")]
        public async Task<RuntimeResult> Reset() => await Task.Run(() =>
            verificationService.ResetVerification(Context.User.Id)
        );

    }
}