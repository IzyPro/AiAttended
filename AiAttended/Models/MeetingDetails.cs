using System;

namespace AiAttended.Models
{
    public class MeetingDetails
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public Guid MeetingId { get; set; }
    }
}
