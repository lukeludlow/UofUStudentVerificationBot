namespace UofUStudentVerificationBot
{
    public interface IRoleAssignmentService
    {
        void AssignVerifiedRoleToDiscordUser(ulong discordID);
        void RemoveVerifiedRoleFromDiscordUser(ulong discordID); 
    }
}