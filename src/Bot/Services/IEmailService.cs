using System.Threading.Tasks;

namespace UofUStudentVerificationBot
{
    public interface IEmailService
    {
        Task SendEmail(string uID, string verificationCode);
    }
}