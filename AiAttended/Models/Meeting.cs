using System;

namespace AiAttended.Models
{
    public class Meeting
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime DateTime { get; set; }
    }
}
