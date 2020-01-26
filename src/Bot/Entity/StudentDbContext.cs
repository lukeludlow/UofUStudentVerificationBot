using System;
using Microsoft.EntityFrameworkCore;

namespace UofUStudentVerificationBot
{
    public class StudentDbContext : DbContext
    {
        public DbSet<Student> Students { get; set; }

        public StudentDbContext()
        {
            // for the default constructor, the OnConfiguring method will be called to configure the database.
        }

        public StudentDbContext(DbContextOptions<StudentDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // TODO i should actually configure the database here but i'm lazy rn
            if (!optionsBuilder.IsConfigured) {
                throw new NotImplementedException();
            }
        }
    }
}