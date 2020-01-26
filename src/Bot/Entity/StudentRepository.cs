using System;
using System.Linq;
using System.Collections.Generic;

namespace UofUStudentVerificationBot
{
    public class StudentRepository : IStudentRepository
    {
        private StudentDbContext dbContext;

        public StudentRepository(StudentDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public StudentRepository()
        {
            this.dbContext = new StudentDbContext();
        }

        public IList<Student> GetAllStudents()
        {
            return dbContext.Students.ToList();
        }

        public Student GetStudentByDiscordID(ulong discordID)
        {
            // return FirstOrDefault! do not return null!!!
            throw new NotImplementedException();
        }

        public void AddOrUpdateStudent(Student student)
        {
            throw new NotImplementedException();
        }

        public void RemoveStudentByDiscordID(ulong discordID)
        {
            throw new NotImplementedException();
        }

    }
}