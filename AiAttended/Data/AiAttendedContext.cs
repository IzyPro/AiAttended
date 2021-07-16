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

        public DbSet<User> Users { get; set; }
        public DbSet<Meeting> Meetings { get; set; }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<Person>().Ignore(x => x.PersistedFaceIds);
        //    base.OnModelCreating(modelBuilder);
        //}
    }
}
