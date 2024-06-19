using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scaffolding.Models;

namespace Scaffolding.Controllers
{
    [Authorize]
    public class FeplDataTypeController : Controller
    {
        private IConfiguration configuration;
        private readonly IWebHostEnvironment webHost;
        private readonly ILogger<FeplDataTypeController> _logger;

        public FeplDataTypeController(IConfiguration _configuration, ILogger<FeplDataTypeController> logger, IWebHostEnvironment _webHost)
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
                List<FeplDataType> records = FeplDataType.Get(Utility.FillStyle.AllProperties, param);
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

                return View(new FeplDataType());
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                return StatusCode(500, "An error occurred while retrieving data from the database.");
            }
        }
        // POST: Create a new record
        [HttpPost]
        public IActionResult Create(FeplDataType model)
        {
            if (model.Save())
            {
                return RedirectToAction("Index");
            }
            else
            {
                return StatusCode(500, "An error occurred while creating a new record.");
            }
        }
        [HttpGet]
        public IActionResult Edit(string id)
        {
            FeplDataType model = FeplDataType.GetById(id, Utility.FillStyle.AllProperties);
            if (model == null)
            {
                return NotFound(); // Return 404 Not Found if the record is not found
            }
            return View(model);
        }
        // POST: Create a new record
        [HttpPost]
        public IActionResult Edit(FeplDataType model)
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
            FeplDataType model = FeplDataType.GetById(id, Utility.FillStyle.AllProperties);
            if (model == null)
            {
                return NotFound(); // Return 404 Not Found if the record is not found
            }
            return View(model);
        }
        // POST: Create a new record
        [HttpPost]
        public IActionResult Delete(FeplDataType model)
        {
            if (model.Delete())
            {
                return RedirectToAction("Index");
            }
            else
            {
                return StatusCode(500, "An error occurred while deleting a new record.");
            }
        }
    }
}