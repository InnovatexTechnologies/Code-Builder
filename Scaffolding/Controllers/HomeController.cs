using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scaffolding.Models;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Scaffolding.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private IConfiguration configuration;


        private readonly IWebHostEnvironment webHost;

        private readonly ILogger<HomeController> _logger;
        public HomeController(IConfiguration _configuration, ILogger<HomeController> logger, IWebHostEnvironment _webHost)
        {
            configuration = _configuration;
            _logger = logger;
            webHost = _webHost;
        }

        public IActionResult Index()
        {              
            List<FeplTable> tables = FeplTable.Get(Utility.FillStyle.WithFullNav);
            return View(tables);
        }



        public IActionResult Download(string id, int mvcApi, int Api)
        {
            try
            {
                string baseFolderPath = "D:\\CodeBuilder";

                if (!Directory.Exists(baseFolderPath))
                    Directory.CreateDirectory(baseFolderPath);

                FeplProject project = FeplProject.GetById(id, Utility.FillStyle.AllProperties);

                if (project == null)
                    return Content("Project not found");

                string projectName = project.name;
                string folderPath = Path.Combine(baseFolderPath, projectName);

                // Create project folder
                Directory.CreateDirectory(folderPath);

                // Generate project files
                GenerateProjectFiles(project, folderPath, mvcApi, Api);

                // Create a ZIP file
                string zipFilePath = Path.Combine("D:\\MyFolder", projectName + ".zip");
                ZipFile.CreateFromDirectory(folderPath, zipFilePath);

                // Read ZIP file bytes
                byte[] fileBytes = System.IO.File.ReadAllBytes(zipFilePath);

                // Delete the temporary folder and ZIP file
                Directory.Delete(folderPath, true);
                System.IO.File.Delete(zipFilePath);

                // Return the ZIP file
                return File(fileBytes, "application/zip", projectName + ".zip");
            }
            catch (Exception ex)
            {
                return Content($"Error: {ex.Message}");
            }
        }

        private void GenerateProjectFiles(FeplProject project, string folderPath, int mvcApi, int Api)
        {
            // Create solution file
            Utility.ExecuteCommand($"dotnet new sln --name {project.name}", folderPath);

            // Create MVC project
            Utility.ExecuteCommand($"dotnet new mvc --name {project.name}", folderPath);
            Utility.ExecuteCommand($"dotnet sln {project.name}.sln add {project.name}", folderPath);



            string projectPath = folderPath + "\\" + project.name;

            // Add required packages
            Utility.ExecuteCommand("dotnet add package System.Data.SQLite", projectPath);
            Utility.ExecuteCommand("dotnet add package Dapper", projectPath);

            Utility.ExecuteCommand("dotnet add package ClosedXML", projectPath);
            Utility.ExecuteCommand("dotnet add package iTextSharp", projectPath);
            Utility.ExecuteCommand("dotnet add package Newtonsoft.Json", projectPath);
            Utility.ExecuteCommand("dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 7.0.18",projectPath);


            // Create database

            Utility.GenerateDataBase(projectPath, "database.db");



            List<FeplTable> tables = FeplTable.Get(Utility.FillStyle.WithFullNav, new Dictionary<string, string>() { { "projectId", project.id } });

            foreach (FeplTable table in tables)
            {
                List<FeplField> fields = Utility.GetFields(table.id);

                // Generate table in database
                using (IDbConnection db = new SQLiteConnection($"Data Source={projectPath}\\AppData\\database.db;foreign keys=true;"))
                {
                    string qry = SqLiteScriptBuilder.GenerateCreateTableScript(fields, table.name);
                    db.Execute(qry);
                }

                // Generate Controller
                if (Api == 0)
                {
                    string controllerFilePath = Path.Combine(projectPath, "Controllers", $"{table.modelName}Controller.cs");
                    string MyController = CSharpCodeBuilder.GenerateControllerCode(fields, table.modelName, table.name, project);
                    System.IO.File.WriteAllText(controllerFilePath, MyController);
                }

                if (mvcApi == 1 || Api == 1)
                {
                    string controllerFilePath = Path.Combine(projectPath, "Controllers", $"{table.modelName}ApiController.cs");
                    string apiMyController = CSharpCodeBuilder.GenerateApiControllerCode(fields, table.modelName, table.name, project);
                    System.IO.File.WriteAllText(controllerFilePath, apiMyController);
                }
                // Generate model
                string modelFilePath = Path.Combine(projectPath, "Models", $"{table.modelName}.cs");
                string MyModel = CSharpCodeBuilder.GenerateModelClass(fields, table.modelName, project);
                System.IO.File.WriteAllText(modelFilePath, MyModel);

                if (Api == 0)
                {
                    // Generate Views
                    string viewsFolder = Path.Combine(projectPath, "Views", table.modelName);
                    Directory.CreateDirectory(viewsFolder);

                    string indexViewFilePath = Path.Combine(viewsFolder, "Index.cshtml");
                    string MyIndexView = CSharpCodeBuilder.GenerateIndexViewCode(fields, table.modelName, project);
                    System.IO.File.WriteAllText(indexViewFilePath, MyIndexView);

                    string createViewFilePath = Path.Combine(viewsFolder, "Create.cshtml");
                    string MyCreateView = CSharpCodeBuilder.GenerateCreateViewCode(fields, table.modelName, project);
                    System.IO.File.WriteAllText(createViewFilePath, MyCreateView);

                    string editViewFilePath = Path.Combine(viewsFolder, "Edit.cshtml");
                    string MyEditView = CSharpCodeBuilder.GenerateEditViewCode(fields, table.modelName, project);
                    System.IO.File.WriteAllText(editViewFilePath, MyEditView);

                    string deleteViewFilePath = Path.Combine(viewsFolder, "Delete.cshtml");
                    string MyDeleteView = CSharpCodeBuilder.GenerateDeleteViewCode(fields, table.modelName, project);
                    System.IO.File.WriteAllText(deleteViewFilePath, MyDeleteView);
                }
            }

            // Generate Utility Class
            string utilityFilePath = Path.Combine(projectPath, "Utility.cs");
            string MyUtilityClass = CSharpCodeBuilder.GenerateUtilityClassCode(project);
            System.IO.File.WriteAllText(utilityFilePath, MyUtilityClass);
            if (Api == 0)
            {
                if (project.isLoginRequired == 1)
                {
                    // Generate Login Controller and View
                    string loginControllerFilePath = Path.Combine(projectPath, "Controllers", "LoginController.cs");
                    string MyLoginController = CSharpCodeBuilder.GenerateLoginController(project.name);
                    System.IO.File.WriteAllText(loginControllerFilePath, MyLoginController);

                    Directory.CreateDirectory(Path.Combine(projectPath, "Views", "Login"));
                    string loginIndexViewFilePath = Path.Combine(projectPath, "Views", "Login", "Index.cshtml");
                    string MyLoginIndexView = CSharpCodeBuilder.GenerateLoginIndexView();
                    System.IO.File.WriteAllText(loginIndexViewFilePath, MyLoginIndexView);
                }
            }
            string programFilePath = Path.Combine(projectPath, "Program.cs");
            string programCode = CSharpCodeBuilder.GenerateProgramCS(project.isLoginRequired, Api, mvcApi);
            System.IO.File.WriteAllText(programFilePath, programCode);

            string appsettingFilePath = Path.Combine(projectPath, "appsettings.json");
            string appsettingCode = CSharpCodeBuilder.GenerateAppSettingsJson();
            System.IO.File.WriteAllText(appsettingFilePath, appsettingCode);
        }
    }
}
