using System;
using Microsoft.AspNetCore.Http;

namespace AiAttended.Models
{
    public class ImageModel
    {
        public string Name { get; set; }
        public IFormFileCollection Images { get; set; }
    }
}
