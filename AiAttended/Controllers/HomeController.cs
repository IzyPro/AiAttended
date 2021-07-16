using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AiAttended.Models;
using AiAttended.Services;

namespace AiAttended.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private IAzureService _azureService;

        public HomeController(ILogger<HomeController> logger, IAzureService azureService)
        {
            _logger = logger;
            _azureService = azureService;
        }

        public IActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> AddPerson([FromForm] AddPersonViewModel model)
        {

            //TempData["AddPersonError"] = "";
            //TempData["AddPersonSuccess"] = "";
            if (!ModelState.IsValid)
            {
                TempData["AddPersonError"] = "Invalid input format";
                return RedirectToAction("Index", "Home");
            }
            var result = await _azureService.AddPersonAsync(model);
            if (result.isSuccess)
            {
                TempData["AddPersonSuccess"] = result.Message;
                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["AddPersonError"] = result.Message;
                return RedirectToAction("Index", "Home");
            }
        }


        [HttpPost]
        public async Task<IActionResult> Train()
        {
            //TempData["TrainError"] = "";
            //TempData["TrainSuccess"] = "";
            var result = await _azureService.TrainGroupAsync();
            if (result.isSuccess)
            {
                TempData["TrainSuccess"] = result.Message;
                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["TrainError"] = result.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Identify([FromForm] AddPersonViewModel model)
        {
            //TempData["IdentifyError"] = "";
            //TempData["IdentifySuccess"] = "";
            var (result, data) = await _azureService.IdentifyFacesAsync(model);
            if (result.isSuccess)
            {
                TempData["IdentifySuccess"] = result.Message;
                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["IdentifyError"] = result.Message;
                return RedirectToAction("Index", "Home");
            }
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
