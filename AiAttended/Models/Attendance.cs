using System;
using System.Collections.Generic;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

namespace AiAttended.Models
{
    public class Attendance
    {
        public DateTime DateTime { get; set; }
        public List<Tuple<Person,double>> Attendees { get;set; }
    }
}
