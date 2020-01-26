using System.ComponentModel.DataAnnotations;

namespace UofUStudentVerificationBot
{
    public class Student
    {
        [Key]
        public ulong DiscordID { get; private set; }
        public string UID { get; private set; }
        public string VerificationCode { get; private set; }
        public bool IsVerificationComplete { get; private set; }

        public Student(ulong discordID, string uID, string verificationCode, bool isVerificationComplete)
        {
            this.DiscordID = discordID;
            this.UID = uID;
            this.VerificationCode = verificationCode;
            this.IsVerificationComplete = isVerificationComplete;
        }

        public Student()
        {
            this.DiscordID = default;
            this.UID = "";
            this.VerificationCode = "";
            this.IsVerificationComplete = false;
        }
    }
}