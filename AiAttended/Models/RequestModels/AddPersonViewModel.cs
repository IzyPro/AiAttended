using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace AiAttended.Models
{
    public class AddPersonViewModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public IFormFileCollection Images { get; set; }
    }
}
