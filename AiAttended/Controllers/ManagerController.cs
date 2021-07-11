using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AiAttended.Models;
using AiAttended.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.Face;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AiAttended.Controllers
{
    public class ManagerController : Controller
    {
        private IAzureService _azureService;
        public ManagerController(IAzureService azureService)
        {
            _azureService = azureService;
        }
        public IActionResult Manager()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddPerson([FromForm] ImageModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Invalid input format";
                return RedirectToAction("Manager", "Manager");
            }
            var result = await _azureService.AddPersonAsync(model);
            if (result.isSuccess)
            {
                ViewBag.Success = result.Message;
                return RedirectToAction("Manager", "Manager");
            }
            else
            {
                ViewBag.Error = result.Message;
                return BadRequest("Unable to add person");
            }
        }


        [HttpPost]
        public async Task<IActionResult> Train()
        {
            var result = await _azureService.TrainGroupAsync();
            if (result.isSuccess)
            {
                //return Ok(result.Message);
                return RedirectToAction("Manager", "Manager");
            }
            else return BadRequest("Failed to train model");
        }

        [HttpPost]
        public async Task<IActionResult> Identify([FromBody] ImageModel model)
        {
            var (result, data) = await _azureService.IdentifyFacesAsync(model);
            if (result.isSuccess)
            {
                return RedirectToAction("Manager", "Manager");
                //return Ok(data);
            }
            else return BadRequest("Failed to identify faces");
        }
    }
}
