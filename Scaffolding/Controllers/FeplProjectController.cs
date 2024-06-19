using System;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scaffolding.Models;

namespace Scaffolding.Controllers
{
    [Authorize]
    public class FeplProjectController : Controller
    {
        private IConfiguration configuration;
        private readonly IWebHostEnvironment webHost;
        private readonly ILogger<FeplProjectController> _logger;

        public FeplProjectController(IConfiguration _configuration, ILogger<FeplProjectController> logger, IWebHostEnvironment _webHost)
        {
            configuration = _configuration;
            _logger = logger;
            webHost = _webHost;
        }
        // List
        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                Dictionary<string, string> param = new Dictionary<string, string>();
                List<FeplProject> records = FeplProject.Get(Utility.FillStyle.AllProperties, param).OrderBy(o => o.name).ToList(); 
                return View(records);
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                return StatusCode(500, "An error occurred while retrieving data from the database.");
            }
        }
        // List
        [HttpPost]
        public IActionResult Index(string id)
        {
            return RedirectToAction("Index");
        }
        // GET: Display form to create a new record
        [HttpGet]
        public IActionResult Create()
        {
            try
            {

                return View(new FeplProject());
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                return StatusCode(500, "An error occurred while retrieving data from the database.");
            }
        }
        // POST: Create a new record
        [HttpPost]
        public IActionResult Create(FeplProject model)
        {
            if (model.Save())
            {
                return RedirectToAction("Index");
            }
            else 
            {
                ViewBag.message = "Project Can't be create"; 
                return StatusCode(500, "An error occurred while creating a new record.");
            }
        }
        [HttpGet]
        public IActionResult Edit(string id)
        {
            FeplProject model = FeplProject.GetById(id, Utility.FillStyle.AllProperties);
            if (model == null)
            {
                return NotFound(); // Return 404 Not Found if the record is not found
            }
            return View(model);
        }
        // POST: Create a new record
        [HttpPost]
        public IActionResult Edit(FeplProject model)
        {
            if (model.Update())
            {
                return RedirectToAction("Index");
            }
            else
            {
                return StatusCode(500, "An error occurred while updating a new record.");
            }
        }
        [HttpGet]
        public IActionResult Delete(string id)
        {
            FeplProject model = FeplProject.GetById(id, Utility.FillStyle.AllProperties);
            if (model == null)
            {
                return NotFound(); // Return 404 Not Found if the record is not found
            }
            return View(model);
        }
        // POST: Create a new record
        [HttpPost]
        public IActionResult Delete(FeplProject model)
        {
            var result = model.Delete();
            if (result.Success)
            {
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.message = $"{result.Exception.Message}";
                //return StatusCode(500, "An error occurred while deleting a new record.");
                 model = FeplProject.GetById(model.id, Utility.FillStyle.AllProperties);
                return View(model);
            }
        }
    }
}