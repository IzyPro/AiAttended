using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AiAttended.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

namespace AiAttended.Services
{
    public interface IAzureService
    {
        Task<ResponseManager> AddPersonAsync(ImageModel model);
        Task<ResponseManager> TrainGroupAsync();
        Task<Tuple<ResponseManager, List<Tuple<Person, double>>>> IdentifyFacesAsync(ImageModel model);
    }
}
