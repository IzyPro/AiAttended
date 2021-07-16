using System;
using System.Collections.Generic;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

namespace AiAttended.Models
{
    public class Meeting
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime DateTime { get; set; }
        public List<User> Attendees { get;set; }
    }
}
