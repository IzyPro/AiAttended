using System;
using System.Linq;
using System.Threading.Tasks;
using AiAttended.Data;
using AiAttended.Models;

namespace AiAttended.Services
{
    public class MeetingService : IMeetingService
    {
        private AiAttendedContext _context;
        public MeetingService(AiAttendedContext context)
        {
            _context = context;
        }

        public async Task<Tuple<ResponseManager, Meeting>> GetAttendees(DateTime dateTime, string name)
        {
            var response = new ResponseManager();
            var people = _context.Users.ToList();
            var meeting = _context.Meetings.Where(x => x.DateTime.Date == dateTime.Date).Where(x => x.Name == name).FirstOrDefault();

            if(people == null)
            {
                response = new ResponseManager
                {
                    isSuccess = false,
                    Message = "No registered persons",
                };
                return new Tuple<ResponseManager, Meeting>(response, null);
            }
            
            if(meeting == null)
            {
                response = new ResponseManager
                {
                    isSuccess = false,
                    Message = "No meeting recorded on this day",
                };
                return new Tuple<ResponseManager, Meeting>(response, null);
            }

            foreach(var person in people)
            {
                if(meeting.Attendees.Where(x => x.Id == person.Id) != null){
                    person.wasPresent = true;
                }
            }

            response = new ResponseManager
            {
                isSuccess = true,
                Message = "Meeting Fetched Successfully",
            };
            return new Tuple<ResponseManager, Meeting>(response, meeting);
        }
    }
}
