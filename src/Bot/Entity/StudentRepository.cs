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

        public IList<Student> GetAllStudents()
        {
            return dbContext.Students.ToList();
        }

        public Student GetStudentByDiscordID(ulong discordID)
        {
            // if not found, returns an empty/default student instead of null
            return dbContext.Students.Find(discordID) ?? new Student();
        }

        public void AddOrUpdateStudent(Student student)
        {
            Student existingStudent = dbContext.Students.Where(s => s.DiscordID == student.DiscordID).FirstOrDefault();
            if (existingStudent == null) {
                dbContext.Students.Add(student);
            } else {
                dbContext.Entry(existingStudent).CurrentValues.SetValues(student);
            }
            dbContext.SaveChanges();
        }

        public void RemoveStudentByDiscordID(ulong discordID)
        {
            Student student = dbContext.Students.Find(discordID);
            if (student != null) {
                dbContext.Students.Remove(student);
                dbContext.SaveChanges();
            }
        }
    }
}