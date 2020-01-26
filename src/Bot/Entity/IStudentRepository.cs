using System.Collections.Generic;

namespace UofUStudentVerificationBot
{
    public interface IStudentRepository
    {
        IList<Student> GetAllStudents();
        Student GetStudentByDiscordID(ulong discordID);
        void AddOrUpdateStudent(Student student);
        void RemoveStudentByDiscordID(ulong discordID);    
    }
}