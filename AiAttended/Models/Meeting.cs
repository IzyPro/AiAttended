using System;
using System.Collections.Generic;

namespace AiAttended.Models
{
    public class Meeting
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime DateTime { get; set; }

        public virtual ICollection<User> Users { get;set; }
    }
}
