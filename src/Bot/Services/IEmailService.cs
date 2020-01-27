namespace UofUStudentVerificationBot
{
    public interface IEmailService
    {
        void SendEmail(string uID, string verificationCode);
    }
}