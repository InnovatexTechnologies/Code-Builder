using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scaffolding.Models;

namespace Scaffolding.Controllers
{
    [Authorize]
    public class FeplTableController : Controller
    {
        private IConfiguration configuration;
        private readonly IWebHostEnvironment webHost;
        private readonly ILogger<FeplTableController> _logger;

        public FeplTableController(IConfiguration _configuration, ILogger<FeplTableController> logger, IWebHostEnvironment _webHost)
        {
            configuration = _configuration;
            _logger = logger;
            webHost = _webHost;
        }
        // List
        [HttpGet]
        public IActionResult Index(string projectId)
        {
            try
            {
                if (!string.IsNullOrEmpty(projectId))
                {
                    ViewBag.projectId = projectId;
                }

                ViewBag.FeplProjects = FeplProject.Get(Utility.FillStyle.Basic).OrderBy(o => o.name).ToList();
                ViewBag.FeplTables = FeplTable.Get(Utility.FillStyle.Basic).OrderBy(o => o.name).ToList();
                ViewBag.FeplDataTypes = FeplDataType.Get(Utility.FillStyle.Basic).OrderBy(o => o.name).ToList();
                ViewBag.FeplFields = FeplField.Get(Utility.FillStyle.Basic).OrderBy(o => o.name).ToList();

                Dictionary<string, string> param = new Dictionary<string, string>();
                param.Add("projectId", projectId);
                List<FeplTable> records = FeplTable.Get(Utility.FillStyle.WithBasicNav, param).OrderBy(o => o.name).ToList();
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
        public IActionResult Index(string id, string projectId)
        {
            return RedirectToAction("Index", new { projectId = projectId });
        }
        // GET: Display form to create a new record
        [HttpGet]
        public IActionResult Create(string projectId,string url)
        {
            try
            {
                ViewBag.url = url;

                ViewBag.projectId = projectId;

                ViewBag.FeplProjects = FeplProject.Get(Utility.FillStyle.AllProperties)
                                 .OrderBy(p => p.name)
                                 .ToList();

                return View(new FeplTable() { projectId = ViewBag.projectId });
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                return StatusCode(500, "An error occurred while retrieving data from the database.");
            }
        }
        // POST: Create a new record
        [HttpPost]
        public IActionResult Create(FeplTable model, string url)
        {
            if (model.Save())
            {
                return Redirect(url);
            }
            else
            {
                return StatusCode(500, "An error occurred while creating a new record.");
            }
        }
        [HttpGet]
        public IActionResult Edit(string id, string url)
        {
            FeplTable model = FeplTable.GetById(id, Utility.FillStyle.WithBasicNav);
            ViewBag.url = url;
            ViewBag.FeplProjects = FeplProject.Get(Utility.FillStyle.AllProperties)
                                 .OrderBy(p => p.name)
                                 .ToList();
            if (model == null)
            {
                return NotFound(); // Return 404 Not Found if the record is not found
            }
            return View(model);
        }
        // POST: Create a new record
        [HttpPost]
        public IActionResult Edit(FeplTable model, string url)
        {
            if (model.Update())
            {
                return Redirect(url);
            }
            else
            {
                return StatusCode(500, "An error occurred while updating a new record.");
            }
        }
        [HttpGet]
        public IActionResult Delete(string id, string url)
        {
            FeplTable model = FeplTable.GetById(id, Utility.FillStyle.WithBasicNav);
            ViewBag.url = url;
            if (model == null)
            {
                return NotFound(); // Return 404 Not Found if the record is not found
            }
            return View(model);
        }
        // POST: Create a new record
        [HttpPost]
        public IActionResult Delete(FeplTable model, string url)
        {
            if (model.Delete())
            {
                return Redirect(url);
            }
            else
            {
                return StatusCode(500, "An error occurred while deleting a new record.");
            }
        }

        public FileContentResult GenerateExcel(string id, string projectId, string tableId, string dataTypeId, string foreignKeyId, string foreignTableId)
        {
            Dictionary<string, string> param1 = new Dictionary<string, string>();
            param1.Add("projectId", projectId);
            string projectName = "";
            projectName = FeplProject.GetById(projectId).name;
            List<FeplTable> tablemodel = FeplTable.Get(Utility.FillStyle.WithBasicNav, param1);
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("tableId", tableId);
            param.Add("dataTypeId", dataTypeId);
            param.Add("foreignKeyId", foreignKeyId);
            param.Add("foreignTableId", foreignTableId);
            List<FeplField> model = FeplField.Get(Utility.FillStyle.WithBasicNav, param);
            int v = 1;
            XLWorkbook wb = new XLWorkbook();
            MemoryStream stream = new MemoryStream();
            DataTable dt = new DataTable();

            foreach (FeplTable a in tablemodel)
            {
                dt = new DataTable(a.name);
                string name = a.name;
                dt.Columns.AddRange(new DataColumn[1]
            {
        new DataColumn(""),
            });
                dt.Rows.Add(name);  
                //dt = new DataTable(a.name);
                dt.Columns.AddRange(new DataColumn[11] 
            {
        new DataColumn("S.No."),
        new DataColumn("Field Name"),
        new DataColumn("Data Type"),
        new DataColumn("Length"),
        new DataColumn("Primary Key"),
        new DataColumn("Required"),
        new DataColumn("Hidden"),
        new DataColumn("Foreign Key"),
        new DataColumn("Print Name"),
        new DataColumn("Default Value"),
        new DataColumn("Foreign Table"),
            });
                v = 1;
              
                foreach (FeplField obj1 in model.Where(o => o.tableNav.id == a.id))
                {
                    dt.Rows.Add(
                        v.ToString(),
                         obj1.name, obj1.dataTypeNav?.name, obj1.length, (obj1.isPrimaryKey == 1 ? "Yes" : "No"), (obj1.isRequired == 1 ? "Yes" : "No"), (obj1.isHidden == 1 ? "Yes" : "No"), obj1.foreignKeyNav?.name, obj1.printName, obj1.defaultValue, obj1.foreignTableNav?.name
                    );
                    v++;
                }


            IXLWorksheet ws = wb.Worksheets.Add(dt);
                wb.SaveAs(stream);
            }
                  return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", projectName+" Table.xlsx");

            //wb.SaveAs(stream);
            //using (XLWorkbook wb = new XLWorkbook())
            //{
               // IXLWorksheet ws = wb.Worksheets.Add(dt);
                //using (MemoryStream stream = new MemoryStream())
                //{
                //    wb.SaveAs(stream);
                //    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "FeplTable.xlsx");
                //}
            //}
        }

        public FileContentResult GeneratePdf(string id, string projectId, string tableId, string dataTypeId, string foreignKeyId, string foreignTableId)
        {
            Dictionary<string, string> param1 = new Dictionary<string, string>();
            param1.Add("projectId", projectId);
            //param1.Add("tableId", tableId);
            //param1.Add("dataTypeId", dataTypeId);
            //param1.Add("foreignKeyId", foreignKeyId);
            //param1.Add("foreignTableId", foreignTableId);
            string projectName = "";
            projectName = FeplProject.GetById(projectId).name;
            List<FeplTable> tablemodel = FeplTable.Get(Utility.FillStyle.WithBasicNav, param1);
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("tableId", tableId);
            param.Add("dataTypeId", dataTypeId);
            param.Add("foreignKeyId", foreignKeyId);
            param.Add("foreignTableId", foreignTableId);
            List<FeplField> model = FeplField.Get(Utility.FillStyle.WithBasicNav, param);
            iTextSharp.text.Font fonta = FontFactory.GetFont("Arial", 8, iTextSharp.text.Font.UNDEFINED, BaseColor.BLACK);
            iTextSharp.text.Font fontb = FontFactory.GetFont("Arial", 18, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
            iTextSharp.text.Font fontc = FontFactory.GetFont("Arial", 9, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
            iTextSharp.text.Font fontd = FontFactory.GetFont("Arial", 11, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
            iTextSharp.text.Font fontn = FontFactory.GetFont("Arial", 14, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
            MemoryStream mmstream = new MemoryStream();
            iTextSharp.text.Document doc = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 15, 15, 15, 15);
            PdfWriter pdfWriter = PdfWriter.GetInstance(doc, mmstream);
            doc.Open();
            PdfContentByte cb = pdfWriter.DirectContent;
            iTextSharp.text.Paragraph report = new iTextSharp.text.Paragraph(projectName, fontb);
            report.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
            report.Font = fontb;
            doc.Add(report);
            //if (tableId != null)
            //{
            //    string a = "";
            //    a = FeplTable.GetById(tableId).name;
            //    string b = "FeplTable";
            //    iTextSharp.text.Paragraph report1 = new iTextSharp.text.Paragraph($"{b}={a}", fontd);
            //    report.Alignment = iTextSharp.text.Element.ALIGN_RIGHT;
            //    report1.Font = fontd;
            //    doc.Add(report1);
            //}
            //if (dataTypeId != null)
            //{
            //    string a = "";
            //    a = FeplDataType.GetById(dataTypeId).name;
            //    string b = "FeplDataType";
            //    iTextSharp.text.Paragraph report1 = new iTextSharp.text.Paragraph($"{b}={a}", fontd);
            //    report.Alignment = iTextSharp.text.Element.ALIGN_RIGHT;
            //    report1.Font = fontd;
            //    doc.Add(report1);
            //}
            //if (foreignKeyId != null)
            //{
            //    string a = "";
            //    a = FeplField.GetById(foreignKeyId).name;
            //    string b = "FeplField";
            //    iTextSharp.text.Paragraph report1 = new iTextSharp.text.Paragraph($"{b}={a}", fontd);
            //    report.Alignment = iTextSharp.text.Element.ALIGN_RIGHT;
            //    report1.Font = fontd;
            //    doc.Add(report1);
            //}
            //if (foreignTableId != null)
            //{
            //    string a = "";
            //    a = FeplTable.GetById(foreignTableId).name;
            //    string b = "FeplTable";
            //    iTextSharp.text.Paragraph report1 = new iTextSharp.text.Paragraph($"{b}={a}", fontd);
            //    report.Alignment = iTextSharp.text.Element.ALIGN_RIGHT;
            //    report1.Font = fontd;
            //    doc.Add(report1);
            //}

            foreach (FeplTable a in tablemodel)
            {
                report = new iTextSharp.text.Paragraph(a.name ?? "", fontd);
                report.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                report.Font = fontb;
                doc.Add(report);
                PdfPTable table = new PdfPTable(11);
                float[] widths = new float[] { .6f, .6f, .6f, .6f, .6f, .6f, .6f, .6f, .6f, .6f, .6f };
                table.SetWidths(widths);
                table.SpacingBefore = 20;
                table.TotalWidth = 560;
                table.LockedWidth = true;
                PdfPCell cell;
                cell = new PdfPCell(new Phrase("SR.No", fontd));
                cell.HorizontalAlignment = 1;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("Field Name", fontd));
                cell.HorizontalAlignment = 1;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("Data Type", fontd));
                cell.HorizontalAlignment = 1;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("Length", fontd));
                cell.HorizontalAlignment = 1;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("Primary Key", fontd));
                cell.HorizontalAlignment = 1;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("Required", fontd));
                cell.HorizontalAlignment = 1;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("Hidden", fontd));
                cell.HorizontalAlignment = 1;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("Foreign Key", fontd));
                cell.HorizontalAlignment = 1;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("Print Name", fontd));
                cell.HorizontalAlignment = 1;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("Default Value", fontd));
                cell.HorizontalAlignment = 1;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("Foreign Table", fontd));
                cell.HorizontalAlignment = 1;
                table.AddCell(cell);
                int v = 1;
                foreach (FeplField obj1 in model.Where(o => o.tableNav.id == a.id))
                {
                    cell = new PdfPCell(new Phrase(v.ToString(), fonta));
                    cell.HorizontalAlignment = 1;
                    table.AddCell(cell);
                    cell = new PdfPCell(new Phrase(obj1.name, fonta));
                    cell.HorizontalAlignment = 1;
                    table.AddCell(cell);
                    cell = new PdfPCell(new Phrase(obj1.dataTypeNav?.name, fonta));
                    cell.HorizontalAlignment = 1;
                    table.AddCell(cell);
                    cell = new PdfPCell(new Phrase(obj1.length.ToString(), fonta));
                    cell.HorizontalAlignment = 1;
                    table.AddCell(cell);
                    cell = new PdfPCell(new Phrase((obj1.isPrimaryKey == 1 ? "Yes" : "No"), fonta));
                    cell.HorizontalAlignment = 1;
                    table.AddCell(cell);
                    cell = new PdfPCell(new Phrase((obj1.isRequired == 1 ? "Yes" : "No"), fonta));
                    cell.HorizontalAlignment = 1;
                    table.AddCell(cell);
                    cell = new PdfPCell(new Phrase((obj1.isHidden == 1 ? "Yes" : "No"), fonta));
                    cell.HorizontalAlignment = 1;
                    table.AddCell(cell);
                    cell = new PdfPCell(new Phrase(obj1.foreignKeyNav?.name, fonta));
                    cell.HorizontalAlignment = 1;
                    table.AddCell(cell);
                    cell = new PdfPCell(new Phrase(obj1.printName, fonta));
                    cell.HorizontalAlignment = 1;
                    table.AddCell(cell);
                    cell = new PdfPCell(new Phrase(obj1.defaultValue, fonta));
                    cell.HorizontalAlignment = 1;
                    table.AddCell(cell);
                    cell = new PdfPCell(new Phrase(obj1.foreignTableNav?.name, fonta));
                    cell.HorizontalAlignment = 1;
                    table.AddCell(cell);
                    v++;
                }
                doc.Add(table);
            }
            pdfWriter.CloseStream = false;
            doc.Close();
            byte[] bytea = mmstream.ToArray();
            return File(bytea, "application/pdf", projectName+" Table.pdf");
        }
    }
}