using System;
using System.Linq;
using System.Threading.Tasks;
using AiAttended.Data;
using AiAttended.Models;
using Microsoft.EntityFrameworkCore;

namespace AiAttended.Services
{
    public class MeetingService : IMeetingService
    {
        private AiAttendedContext _context;
        public MeetingService(AiAttendedContext context)
        {
            _context = context;
        }

        public async Task<Tuple<ResponseManager, MeetingViewModel>> GetAttendees(DateTime dateTime, string name)
        {
            try
            {
                var response = new ResponseManager();
                var people = await _context.Users.ToListAsync();
                var meeting = await _context.Meetings.FirstOrDefaultAsync(x => x.DateTime.Date == dateTime.Date && x.Name == name);

                if (people == null)
                {
                    response = new ResponseManager
                    {
                        isSuccess = false,
                        Message = "No registered persons",
                    };
                    return new Tuple<ResponseManager, MeetingViewModel>(response, null);
                }

                if (meeting == null)
                {
                    response = new ResponseManager
                    {
                        isSuccess = false,
                        Message = $"No meeting with name {name} recorded on this day",
                    };
                    return new Tuple<ResponseManager, MeetingViewModel>(response, null);
                }

                var meetingDetails = _context.MeetingDetails.Where(x => x.MeetingId == meeting.Id);

                foreach (var person in people)
                {
                    if (meetingDetails.FirstOrDefault(x => x.UserId == person.Id) != null)
                    {
                        person.wasPresent = true;
                    }
                }

                var meetingViewModel = new MeetingViewModel
                {
                    DateTime = meeting.DateTime,
                    Users = people,
                };

                response = new ResponseManager
                {
                    isSuccess = true,
                    Message = "Meeting Fetched Successfully",
                };
                return new Tuple<ResponseManager, MeetingViewModel>(response, meetingViewModel);
            }
            catch(Exception ex)
            {
                var response = new ResponseManager
                {
                    isSuccess = false,
                    Message = ex.Message,
                };
                return new Tuple<ResponseManager, MeetingViewModel>(response, null);
            }
        }
    }
}
