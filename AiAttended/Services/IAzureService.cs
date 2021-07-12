using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AiAttended.Models;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

namespace AiAttended.Services
{
    public interface IAzureService
    {
        Task<ResponseManager> AddPersonAsync(AddPersonViewModel model);
        Task<ResponseManager> TrainGroupAsync();
        Task<Tuple<ResponseManager, List<Tuple<Person, double>>>> IdentifyFacesAsync(AddPersonViewModel model);
    }
}
