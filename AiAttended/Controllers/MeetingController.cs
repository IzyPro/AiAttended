using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AiAttended.Data;
using AiAttended.Services;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AiAttended.Controllers
{
    public class MeetingController : Controller
    {
        private IMeetingService _meetingService;
        public MeetingController(IMeetingService meetingService)
        {
            _meetingService = meetingService;
        }
        [HttpGet]
        public IActionResult Meeting()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Meeting(DateTime dateTime, string name)
        {
            var (response, result) = await _meetingService.GetAttendees(dateTime, name);
            if (!response.isSuccess)
            {
                ViewBag.Error = response.Message;
                return View();
            }
            ViewBag.Meeting = result;
            return View();
        }
    }
}
