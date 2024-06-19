using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Data;
using System.Data.SQLite;
using Dapper;
using Scaffolding.Models;
using Microsoft.AspNetCore.Authorization;

namespace Scaffolding.Controllers
{
    [Authorize]
    public class FeplFieldController : Controller
    {
        private IConfiguration configuration;
        private readonly IWebHostEnvironment webHost;
        private readonly ILogger<FeplFieldController> _logger;

        public FeplFieldController(IConfiguration _configuration, ILogger<FeplFieldController> logger, IWebHostEnvironment _webHost)
        {
            configuration = _configuration;
            _logger = logger;
            webHost = _webHost;
        }
        // List
        [HttpGet]
        public IActionResult Index(string projectId, string tableId, string dataTypeId, string foreignKeyId, string foreignTableId, string metaTableId)
        {
            try
            {
                if (!string.IsNullOrEmpty(projectId))
                {
                    ViewBag.projectId = projectId;
                }
                if (!string.IsNullOrEmpty(tableId))
                {
                    ViewBag.tableId = tableId;
                }
                if (!string.IsNullOrEmpty(dataTypeId))
                {
                    ViewBag.dataTypeId = dataTypeId;
                }
                if (!string.IsNullOrEmpty(foreignKeyId))
                {
                    ViewBag.foreignKeyId = foreignKeyId;
                }
                if (!string.IsNullOrEmpty(foreignTableId))
                {
                    ViewBag.foreignTableId = foreignTableId;
                }
                if (!string.IsNullOrEmpty(metaTableId))
                {
                    ViewBag.metaTableId = metaTableId;
                }
                Dictionary<string, string> paramn = new Dictionary<string, string>();
                paramn.Add("projectId", projectId);
                ViewBag.FeplTables = FeplTable.Get(Utility.FillStyle.Basic, paramn).OrderBy(o => o.name).ToList();
                ViewBag.FeplDataTypes = FeplDataType.Get(Utility.FillStyle.Basic).OrderBy(o => o.name).ToList();
                ViewBag.FeplFields = FeplField.Get(Utility.FillStyle.Basic).OrderBy(o => o.name).ToList();

                Dictionary<string, string> param = new Dictionary<string, string>();
                param.Add("tableId", tableId);
                param.Add("dataTypeId", dataTypeId);
                param.Add("foreignKeyId", foreignKeyId);
                param.Add("foreignTableId", foreignTableId);
                param.Add("metaTableId", metaTableId);
                List<FeplField> records = FeplField.Get(Utility.FillStyle.WithBasicNav, param).OrderBy(o => o.name).ToList();
                return View(records);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving data from the database.");
            }
        }
        // List   
        [HttpPost]
        public IActionResult Index(string id, string projectId, string tableId, string dataTypeId, string foreignKeyId, string foreignTableId, string metaTableId)
        {
            return RedirectToAction("Index", new { projectId = projectId, tableId = tableId, dataTypeId = dataTypeId, foreignKeyId = foreignKeyId, foreignTableId = foreignTableId, metaTableId = metaTableId });
        }
        // GET: Display form to create a new record
        [HttpGet]
        public IActionResult Create(string projectId, string tableId, string url)
        {
            try
            {
                ViewBag.url = url;
                ViewBag.projectId = projectId;
                ViewBag.tableId = tableId;
                Dictionary<string, string> param = new Dictionary<string, string>();
                param.Add("projectId", projectId);
                ViewBag.FeplTables = FeplTable.Get(Utility.FillStyle.AllProperties, param).OrderBy(o => o.name).ToList();
                ViewBag.FeplDataTypes = FeplDataType.Get(Utility.FillStyle.AllProperties).OrderBy(o => o.name).ToList();
                ViewBag.FeplFields = FeplField.Get(Utility.FillStyle.AllProperties).OrderBy(o => o.name).ToList();



                return View(new FeplField() { tableId = ViewBag.tableId });
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                return StatusCode(500, "An error occurred while retrieving data from the database.");
            }
        }
        // POST: Create a new record
        [HttpPost]
        public IActionResult Create(FeplField model, string url)
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
        public IActionResult Edit(string projectId, string id, string url)
        {
            FeplField model = FeplField.GetById(id, Utility.FillStyle.WithBasicNav);
            ViewBag.url = url;
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("projectId", projectId);
            ViewBag.FeplTables = FeplTable.Get(Utility.FillStyle.AllProperties, param).OrderBy(o => o.name).ToList();
            ViewBag.FeplDataTypes = FeplDataType.Get(Utility.FillStyle.AllProperties).OrderBy(o => o.name).ToList();
            ViewBag.FeplFields = FeplField.Get(Utility.FillStyle.AllProperties).OrderBy(o => o.name).ToList();
            if (model == null)
            {
                return NotFound(); // Return 404 Not Found if the record is not found
            }
            return View(model);
        }
        // POST: Create a new record
        [HttpPost]
        public IActionResult Edit(FeplField model, string url)
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
            FeplField model = FeplField.GetById(id, Utility.FillStyle.WithBasicNav);
            ViewBag.url = url;
            if (model == null)
            {
                return NotFound(); // Return 404 Not Found if the record is not found
            }
            return View(model);
        }
        // POST: Create a new record
        [HttpPost]
        public IActionResult Delete(FeplField model, string url)
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
        public FileContentResult GenerateExcel(string id, string tableId, string dataTypeId, string foreignKeyId, string foreignTableId)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("tableId", tableId);
            param.Add("dataTypeId", dataTypeId);
            param.Add("foreignKeyId", foreignKeyId);
            param.Add("foreignTableId", foreignTableId);
            List<FeplField> model = FeplField.Get(Utility.FillStyle.WithBasicNav, param);
            int v = 1;
            DataTable dt = new DataTable("FeplFields");
            dt.Columns.AddRange(new DataColumn[13]
            {
    new DataColumn("S.No."),
    new DataColumn("tableId"),
    new DataColumn("fieldName"),
    new DataColumn("dataTypeId"),
    new DataColumn("length"),
    new DataColumn("isPrimaryKey"),
    new DataColumn("isRequired"),
    new DataColumn("isHidden"),
    new DataColumn("metaRequired"),
    new DataColumn("foreignKeyId"),
    new DataColumn("printName"),
    new DataColumn("Default Value"),
    new DataColumn("Foreign Table"),
            });
            foreach (FeplField obj1 in model)
            {
                dt.Rows.Add(
                    v.ToString(),
                     obj1.tableNav?.name, obj1.name, obj1.dataTypeNav?.name, obj1.length, (obj1.isPrimaryKey == 1 ? "Yes" : "No"), (obj1.isRequired == 1 ? "Yes" : "No"), (obj1.isHidden == 1 ? "Yes" : "No"), (obj1.metaRequired == 1 ? "Yes" : "No"), obj1.foreignKeyNav?.name, obj1.printName, obj1.defaultValue, obj1.foreignTableNav?.name
                );
                v++;
            }
            using (XLWorkbook wb = new XLWorkbook())
            {
                IXLWorksheet ws = wb.Worksheets.Add(dt);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "FeplField.xlsx");
                }
            }
        }
        public FileContentResult GeneratePdf(string id, string tableId, string dataTypeId, string foreignKeyId, string foreignTableId)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("tableId", tableId);
            param.Add("dataTypeId", dataTypeId);
            param.Add("foreignKeyId", foreignKeyId);
            param.Add("foreignTableId", foreignTableId);
            List<FeplField> model = FeplField.Get(Utility.FillStyle.WithBasicNav, param);
            iTextSharp.text.Font fonta = FontFactory.GetFont("Arial", 8, iTextSharp.text.Font.UNDEFINED, BaseColor.BLACK);
            iTextSharp.text.Font fontb = FontFactory.GetFont("Arial", 18, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
            iTextSharp.text.Font fontc = FontFactory.GetFont("Arial", 9, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
            iTextSharp.text.Font fontd = FontFactory.GetFont("Arial", 12, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
            MemoryStream mmstream = new MemoryStream();
            iTextSharp.text.Document doc = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 15, 15, 15, 15);
            PdfWriter pdfWriter = PdfWriter.GetInstance(doc, mmstream);
            doc.Open();
            PdfContentByte cb = pdfWriter.DirectContent;
            iTextSharp.text.Paragraph report = new iTextSharp.text.Paragraph("FeplFields", fontb);
            report.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
            report.Font = fontb;
            doc.Add(report);
            if (tableId != null)
            {
                string a = "";
                a = FeplTable.GetById(tableId).name;
                string b = "FeplTable";
                iTextSharp.text.Paragraph report1 = new iTextSharp.text.Paragraph($"{b}={a}", fontd);
                report.Alignment = iTextSharp.text.Element.ALIGN_RIGHT;
                report1.Font = fontd;
                doc.Add(report1);
            }
            if (dataTypeId != null)
            {
                string a = "";
                a = FeplDataType.GetById(dataTypeId).name;
                string b = "FeplDataType";
                iTextSharp.text.Paragraph report1 = new iTextSharp.text.Paragraph($"{b}={a}", fontd);
                report.Alignment = iTextSharp.text.Element.ALIGN_RIGHT;
                report1.Font = fontd;
                doc.Add(report1);
            }
            if (foreignKeyId != null)
            {
                string a = "";
                a = FeplField.GetById(foreignKeyId).name;
                string b = "FeplField";
                iTextSharp.text.Paragraph report1 = new iTextSharp.text.Paragraph($"{b}={a}", fontd);
                report.Alignment = iTextSharp.text.Element.ALIGN_RIGHT;
                report1.Font = fontd;
                doc.Add(report1);
            }
            if (foreignTableId != null)
            {
                string a = "";
                a = FeplTable.GetById(foreignTableId).name;
                string b = "FeplTable";
                iTextSharp.text.Paragraph report1 = new iTextSharp.text.Paragraph($"{b}={a}", fontd);
                report.Alignment = iTextSharp.text.Element.ALIGN_RIGHT;
                report1.Font = fontd;
                doc.Add(report1);
            }
            PdfPTable table = new PdfPTable(13);
            float[] widths = new float[] { .6f, .6f, .6f, .6f, .6f, .6f, .6f, .6f, .6f, .6f, .6f, .6f, .6f };
            table.SetWidths(widths);
            table.SpacingBefore = 20;
            table.TotalWidth = 560;
            table.LockedWidth = true;
            PdfPCell cell;
            cell = new PdfPCell(new Phrase("SR.No", fontd));
            cell.HorizontalAlignment = 1;
            table.AddCell(cell);
            cell = new PdfPCell(new Phrase("tableId", fontd));
            cell.HorizontalAlignment = 1;
            table.AddCell(cell);
            cell = new PdfPCell(new Phrase("fieldName", fontd));
            cell.HorizontalAlignment = 1;
            table.AddCell(cell);
            cell = new PdfPCell(new Phrase("dataTypeId", fontd));
            cell.HorizontalAlignment = 1;
            table.AddCell(cell);
            cell = new PdfPCell(new Phrase("length", fontd));
            cell.HorizontalAlignment = 1;
            table.AddCell(cell);
            cell = new PdfPCell(new Phrase("isPrimaryKey", fontd));
            cell.HorizontalAlignment = 1;
            table.AddCell(cell);
            cell = new PdfPCell(new Phrase("isRequired", fontd));
            cell.HorizontalAlignment = 1;
            table.AddCell(cell);
            cell = new PdfPCell(new Phrase("isHidden", fontd));
            cell.HorizontalAlignment = 1;
            table.AddCell(cell);
            cell = new PdfPCell(new Phrase("metaRequired", fontd));
            cell.HorizontalAlignment = 1;
            table.AddCell(cell);
            cell = new PdfPCell(new Phrase("foreignKeyId", fontd));
            cell.HorizontalAlignment = 1;
            table.AddCell(cell);
            cell = new PdfPCell(new Phrase("printName", fontd));
            cell.HorizontalAlignment = 1;
            table.AddCell(cell);
            cell = new PdfPCell(new Phrase("Default Value", fontd));
            cell.HorizontalAlignment = 1;
            table.AddCell(cell);
            cell = new PdfPCell(new Phrase("Foreign Table", fontd));
            cell.HorizontalAlignment = 1;
            table.AddCell(cell);
            int v = 1;
            foreach (FeplField obj1 in model)
            {
                cell = new PdfPCell(new Phrase(v.ToString(), fonta));
                cell.HorizontalAlignment = 1;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase(obj1.tableNav?.name, fonta));
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
                cell = new PdfPCell(new Phrase((obj1.metaRequired == 1 ? "Yes" : "No"), fonta));
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
            pdfWriter.CloseStream = false;
            doc.Close();
            byte[] bytea = mmstream.ToArray();
            return File(bytea, "application/pdf", "FeplField.pdf");
        }
    }
}
