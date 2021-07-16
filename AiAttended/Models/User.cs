using System;
using System.Collections.Generic;

namespace AiAttended.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string UserData { get; set; }
        public string UserGroupId { get; set; }
        public List<Guid>? PersistedFaceIDs { get; set; }
        public bool? wasPresent { get; set; }
        public DateTime Created { get; set; }
    }
}
