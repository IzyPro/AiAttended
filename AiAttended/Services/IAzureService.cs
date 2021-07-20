using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AiAttended.Models;

namespace AiAttended.Services
{
    public interface IAzureService
    {
        Task<ResponseManager> AddPersonAsync(AddPersonViewModel model);
        Task<ResponseManager> TrainGroupAsync();
        Task<Tuple<ResponseManager, List<User>>> IdentifyFacesAsync(AddPersonViewModel model);
    }
}
