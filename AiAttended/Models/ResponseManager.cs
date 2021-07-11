using System;
using System.Collections.Generic;

namespace AiAttended.Models
{
    public class ResponseManager
    {
        public string Message { get; set; }
        public bool isSuccess { get; set; }
        public IEnumerable<string> Errors { get; set; }
    }
}
