using System;
namespace AiAttended.Models
{
    public class PersonGroupResponseModel
    {
        public Guid LargePersonGroupId { get; set; }

        public string Name { get; set; }

        public string UserData { get; set; }

        public string RecognitionModel { get; set; }
    }
}
