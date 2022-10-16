using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AiAttended.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string UserData { get; set; }
        public string UserGroupId { get; set; }
        public bool? wasPresent { get; set; }
        public DateTime Created { get; set; }
    }
}
