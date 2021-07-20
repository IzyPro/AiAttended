using System;
using System.Collections.Generic;

namespace AiAttended.Models
{
    public class MeetingViewModel
    {
        public DateTime DateTime { get; set; }
        public List<User> Users { get; set; }
    }
}
