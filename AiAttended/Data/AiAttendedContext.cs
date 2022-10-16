using AiAttended.Models;
using Microsoft.EntityFrameworkCore;

namespace AiAttended.Data
{
    public class AiAttendedContext : DbContext
    {
        public AiAttendedContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Meeting> Meetings { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<MeetingDetails> MeetingDetails { get; set; }
    }
}
