using System;
using AiAttended.Models;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.EntityFrameworkCore;

namespace AiAttended.Data
{
    public class AiAttendedContext : DbContext
    {
        public AiAttendedContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Person> People { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
    }
}
