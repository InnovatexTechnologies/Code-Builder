using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Data;
using System.Data.SQLite;
using Dapper;
using Scaffolding.Models;

namespace Scaffolding.Controllers
{
[Authorize]
public class LoginUserController : Controller
{
	private IConfiguration configuration;
	private readonly IWebHostEnvironment webHost;
	private readonly ILogger<LoginUserController> _logger;

	public LoginUserController(IConfiguration _configuration, ILogger<LoginUserController> logger, IWebHostEnvironment _webHost)
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


Dictionary<string,string> param = new Dictionary<string, string>();
				List<LoginUser> records = LoginUser.Get(Utility.FillStyle.AllProperties, param);
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
	public IActionResult Index(string id )
	{
				 return RedirectToAction("Index");
	}
	// GET: Display form to create a new record
	[HttpGet]
	public IActionResult Create(string url)
	{
		try
		{

				ViewBag.url= url;
				return View(new LoginUser());
			}
		catch (Exception ex)
		{
			// Log the exception or handle it as needed
			return StatusCode(500, "An error occurred while retrieving data from the database.");
		}
	}
	// POST: Create a new record
	[HttpPost]
	public IActionResult Create(LoginUser model, string url)
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
	LoginUser model = LoginUser.GetById(id,Utility.FillStyle.AllProperties);
				ViewBag.url = url;
	if (model == null)
		{
		return NotFound(); // Return 404 Not Found if the record is not found
		}
		return View(model);
	}
	// POST: Create a new record
	[HttpPost]
	public IActionResult Edit(LoginUser model, string url)
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
	LoginUser model = LoginUser.GetById(id,Utility.FillStyle.AllProperties);
	ViewBag.url = url;
	if (model == null)
		{
		return NotFound(); // Return 404 Not Found if the record is not found
		}
		return View(model);
	}
	// POST: Create a new record
	[HttpPost]
	public IActionResult Delete(LoginUser model,string url)
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
public FileContentResult GenerateExcel(string id )
{
Dictionary<string,string> param = new Dictionary<string, string>();
				List<LoginUser> model = LoginUser.Get(Utility.FillStyle.AllProperties, param);
int v = 1;
DataTable dt = new DataTable("LoginUsers");
dt.Columns.AddRange(new DataColumn[1]
{
    new DataColumn("S.No."),
});
foreach (LoginUser obj1 in model)
{
    dt.Rows.Add(
        v.ToString()
    );
    v++;
}
using (XLWorkbook wb = new XLWorkbook())
{
    IXLWorksheet ws = wb.Worksheets.Add(dt);
    using (MemoryStream stream = new MemoryStream())
    {
        wb.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "LoginUser.xlsx");
    }
}
}
public FileContentResult GeneratePdf(string id )
{
Dictionary<string,string> param = new Dictionary<string, string>();
				List<LoginUser> model = LoginUser.Get(Utility.FillStyle.AllProperties, param);
iTextSharp.text.Font fonta = FontFactory.GetFont("Arial", 8, iTextSharp.text.Font.UNDEFINED, BaseColor.BLACK);
iTextSharp.text.Font fontb = FontFactory.GetFont("Arial", 18, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
iTextSharp.text.Font fontc = FontFactory.GetFont("Arial", 9, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
iTextSharp.text.Font fontd = FontFactory.GetFont("Arial", 12, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
MemoryStream mmstream = new MemoryStream();
iTextSharp.text.Document doc = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 15, 15, 15, 15);
PdfWriter pdfWriter = PdfWriter.GetInstance(doc, mmstream);
doc.Open();
PdfContentByte cb = pdfWriter.DirectContent;
iTextSharp.text.Paragraph report = new iTextSharp.text.Paragraph("LoginUsers", fontb);
report.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
report.Font = fontb;
doc.Add(report);
PdfPTable table = new PdfPTable(1);
float[] widths = new float[] {.6f};
table.SetWidths(widths);
table.SpacingBefore = 20;
table.TotalWidth = 560;
table.LockedWidth = true;
PdfPCell cell;
cell = new PdfPCell(new Phrase("SR.No", fontd));
cell.HorizontalAlignment = 1;
table.AddCell(cell);
int v = 1;
foreach (LoginUser obj1 in model)
{
    cell = new PdfPCell(new Phrase(v.ToString(), fonta));
    cell.HorizontalAlignment = 1;
    table.AddCell(cell);
    v++;
}
doc.Add(table);
pdfWriter.CloseStream = false;
doc.Close();
byte[] bytea = mmstream.ToArray();
return File(bytea, "application/pdf", "LoginUser.pdf");
   }
  }
 }
