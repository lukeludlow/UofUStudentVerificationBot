using Discord.Commands;

namespace UofUStudentVerificationBot
{
    public class VerificationResult : RuntimeResult
    {
        public VerificationResult(CommandError? error, string reason) 
            : base(error, reason)
        {
        }
        public static VerificationResult FromSuccess(string successMessage = "") => new VerificationResult(null, successMessage);
        public static VerificationResult FromError(string errorMessage) => new VerificationResult(CommandError.Unsuccessful, errorMessage);
    }
}