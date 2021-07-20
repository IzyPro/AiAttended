using System;
using System.Threading.Tasks;
using AiAttended.Models;

namespace AiAttended.Services
{
    public interface IMeetingService
    {
        Task<Tuple<ResponseManager, MeetingViewModel>> GetAttendees(DateTime dateTime, string name);
    }
}
