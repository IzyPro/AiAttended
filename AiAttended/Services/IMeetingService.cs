using System;
using System.Threading.Tasks;
using AiAttended.Models;

namespace AiAttended.Services
{
    public interface IMeetingService
    {
        Task<Tuple<ResponseManager, Meeting>> GetAttendees(DateTime dateTime, string name);
    }
}
