using Scaffolding.Controllers;
using Scaffolding.Models;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.Data;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.AspNetCore.Http.HttpResults;
using DocumentFormat.OpenXml.Wordprocessing;
using static System.Runtime.InteropServices.JavaScript.JSType;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore;
using DocumentFormat.OpenXml.Math;
using System.Linq;
using DocumentFormat.OpenXml.Drawing.Charts;
using iTextSharp.text;
using Dapper;
using DocumentFormat.OpenXml.EMMA;

namespace Scaffolding
{
    public static class CSharpCodeBuilder
    {
        public static string GenerateModelClass(List<FeplField> fields, string modelName, FeplProject project)
        {
            StringBuilder codeBuilder = new StringBuilder();

            // Generate model class header
            codeBuilder.AppendLine($"using System.Data.SQLite;");
            codeBuilder.AppendLine($"using System.Data;");
            codeBuilder.AppendLine($"using Dapper;");
            codeBuilder.AppendLine($"using System.ComponentModel.DataAnnotations.Schema;");
            codeBuilder.AppendLine("");
            codeBuilder.AppendLine("");
            codeBuilder.AppendLine($"namespace {project.name}.Models");
            codeBuilder.AppendLine("{");

            // Generate properties for each field
            codeBuilder.AppendLine($"\tpublic class {modelName}");
            codeBuilder.AppendLine("\t{");

            foreach (FeplField field in fields)
            {
                if ((field.dataTypeNav.name == "Boolean" || field.dataTypeNav.name == "Date" || field.dataTypeNav.name == "Integer" || field.dataTypeNav.name == "Decimal") && string.IsNullOrEmpty(field.defaultValue))
                {
                    codeBuilder.AppendLine($"\t\tpublic {field.dataTypeNav.cSharp} {field.name} {{ get; set; }}= 0; ");
                }
                else if (field.dataTypeNav.name == "Boolean" || field.dataTypeNav.name == "Date" || field.dataTypeNav.name == "Integer" || field.dataTypeNav.name == "Decimal")
                {
                    codeBuilder.AppendLine($"\t\tpublic {field.dataTypeNav.cSharp} {field.name} {{ get; set; }}= {field.defaultValue};");
                }
                else if (string.IsNullOrEmpty(field.defaultValue))
                {
                    codeBuilder.AppendLine($"\t\tpublic {field.dataTypeNav.cSharp} {field.name} {{ get; set; }} = \"\";");
                }
                else
                {
                    codeBuilder.AppendLine($"\t\tpublic {field.dataTypeNav.cSharp} {field.name} {{ get; set; }}= \"{field.defaultValue}\";");
                }

                if (!string.IsNullOrEmpty(field.foreignKeyId))
                {
                    codeBuilder.AppendLine($"\t\tpublic {field.foreignKeyNav.tableNav.modelName} {field.name.TrimId()}Nav {{ get; set; }} = new {field.foreignKeyNav.tableNav.modelName}();");
                }
                if (field.metaRequired == 1)
                {
                    codeBuilder.AppendLine("\t\t[NotMapped]");
                    codeBuilder.AppendLine($"\t\tpublic List<{fields.Where(o => o.metaRequired == 1).FirstOrDefault().printName}> {fields.Where(o => o.metaRequired == 1).FirstOrDefault().printName.ToLower()} {{ get; set; }} = new List<{fields.Where(o => o.metaRequired == 1).FirstOrDefault().printName}>();");
                }
            }

            // Additional methods
            codeBuilder.AppendLine("\t// Additional methods");

            // GetAll method
            codeBuilder.AppendLine($"\tpublic static List<{modelName}> Get(Utility.FillStyle fillStyle = Utility.FillStyle.AllProperties, Dictionary<string, string>? paramList = null)");
            codeBuilder.AppendLine("\t{");
            codeBuilder.AppendLine("\t\tusing (IDbConnection db = new SQLiteConnection(Utility.ConnString))");
            codeBuilder.AppendLine("\t\t{");
            codeBuilder.AppendLine("\t\t\tdb.Open();");

            // Add code here
            codeBuilder.AppendLine("\t\t\tstring whereClause = \"1=1\";");

            codeBuilder.AppendLine("\t\t\t if (paramList == null)");
            codeBuilder.AppendLine("\t\t\t{");
            codeBuilder.AppendLine("\t\t\t paramList = new Dictionary<string, string>();");
            codeBuilder.AppendLine("\t\t\t}");

            codeBuilder.AppendLine("\t\t\tforeach (var obj in paramList)");
            codeBuilder.AppendLine("\t\t\t{");
            codeBuilder.AppendLine("\t\t\t\tif (!string.IsNullOrEmpty(obj.Value))");
            codeBuilder.AppendLine("\t\t\t\t{");
            codeBuilder.AppendLine("\t\t\t\t\twhereClause += \" and myBaseTable.\" + obj.Key + \" = @\" + obj.Key;");
            codeBuilder.AppendLine("\t\t\t\t}");
            codeBuilder.AppendLine("\t\t\t}");



            codeBuilder.AppendLine("\t\t\tstring query = \"\";");


            codeBuilder.AppendLine("\t\t\tif (fillStyle == Utility.FillStyle.Basic)");
            codeBuilder.AppendLine("\t\t\t{");
            codeBuilder.AppendLine($"\t\t\t\tquery = \"{Utility.SelectQuery(fields.FirstOrDefault()?.tableNav?.id ?? "", Utility.FillStyle.Basic)}\";");
            Type type = Type.GetType(modelName);
            if (type != null && type.GetProperty("name") != null)
            {
                codeBuilder.AppendLine($"\t\t\treturn db.Query<{modelName}>(query).OrderBy(o => o.name).ToList();");
            }
            else
            {
                codeBuilder.AppendLine($"\t\t\treturn db.Query<{modelName}>(query).ToList();");
            }
            codeBuilder.AppendLine("\t\t\t}");
            codeBuilder.AppendLine("\t\t\telse if (fillStyle == Utility.FillStyle.AllProperties)");
            codeBuilder.AppendLine("\t\t\t{");
            codeBuilder.AppendLine($"\t\t\t\tquery = \"{Utility.SelectQuery(fields.FirstOrDefault()?.tableNav?.id ?? "", Utility.FillStyle.AllProperties)}\";");
            type = Type.GetType(modelName);
            if (type != null && type.GetProperty("name") != null)
            {
                codeBuilder.AppendLine($"\t\t\treturn db.Query<{modelName}>(query).OrderBy(o => o.name).ToList();");
            }
            else
            {
                codeBuilder.AppendLine($"\t\t\treturn db.Query<{modelName}>(query).ToList();");
            }
            codeBuilder.AppendLine("\t\t\t}");
            if (fields.Where(fld => !string.IsNullOrEmpty(fld.foreignKeyId)).Count() > 0)
            {
                codeBuilder.AppendLine("\t\t\telse if (fillStyle == Utility.FillStyle.WithBasicNav)");
                codeBuilder.AppendLine("\t\t\t{");
                codeBuilder.AppendLine($"\t\t\t\tquery = \"{Utility.SelectQuery(fields.FirstOrDefault()?.tableNav?.id ?? "", Utility.FillStyle.WithBasicNav)} where \"+whereClause ;");

                codeBuilder.Append($"\t\t\t\treturn db.Query<{modelName}");

                foreach (FeplField field in fields.Where(fld => !string.IsNullOrEmpty(fld.foreignKeyId)))
                {
                    codeBuilder.Append($", {field.foreignKeyNav.tableNav.modelName}");
                }
                codeBuilder.AppendLine($", {modelName}>(query,");

                List<string> tableList = new List<string>();
                tableList.Add("myBaseTable");

                int refNo = 1;
                foreach (FeplField field in fields.Where(fld => !string.IsNullOrEmpty(fld.foreignKeyId)))
                {
                    tableList.Add("ref" + refNo);
                    refNo++;
                }

                codeBuilder.AppendLine($"\t\t\t\t({string.Join(",", tableList)}) =>");
                codeBuilder.AppendLine($"\t\t\t\t{{");

                refNo = 1;
                foreach (FeplField field in fields.Where(fld => !string.IsNullOrEmpty(fld.foreignKeyId)))
                {
                    codeBuilder.AppendLine($"\t\t\t\t\tmyBaseTable.{field.name.TrimId()}Nav =ref{refNo} ;");
                    refNo++;
                }


                codeBuilder.AppendLine($"\t\t\t\t\treturn myBaseTable;");
                codeBuilder.AppendLine($"\t\t\t\t}},Utility.CreateDynamicObject(paramList)).ToList();");

                codeBuilder.AppendLine("\t\t\t}");
                codeBuilder.AppendLine("\t\t\telse if (fillStyle == Utility.FillStyle.WithFullNav)");
                codeBuilder.AppendLine("\t\t\t{");
                codeBuilder.AppendLine($"\t\t\t\tquery = \"{Utility.SelectQuery(fields.FirstOrDefault()?.tableNav?.id ?? "", Utility.FillStyle.WithFullNav)} where \"+whereClause ;");

                codeBuilder.Append($"\t\t\t\treturn db.Query<{modelName}");

                foreach (FeplField field in fields.Where(fld => !string.IsNullOrEmpty(fld.foreignKeyId)))
                {
                    codeBuilder.Append($", {field.foreignKeyNav.tableNav.modelName}");
                }
                codeBuilder.AppendLine($", {modelName}>(query,");

                tableList = new List<string>();
                tableList.Add("myBaseTable");

                refNo = 1;
                foreach (FeplField field in fields.Where(fld => !string.IsNullOrEmpty(fld.foreignKeyId)))
                {
                    tableList.Add("ref" + refNo);
                    refNo++;
                }

                codeBuilder.AppendLine($"\t\t\t\t({string.Join(",", tableList)}) =>");
                codeBuilder.AppendLine($"\t\t\t\t{{");

                refNo = 1;
                foreach (FeplField field in fields.Where(fld => !string.IsNullOrEmpty(fld.foreignKeyId)))
                {
                    codeBuilder.AppendLine($"\t\t\t\t\tmyBaseTable.{field.name.TrimId()}Nav =ref{refNo} ;");
                    refNo++;
                }
                codeBuilder.AppendLine($"\t\t\t\t\treturn myBaseTable;");
                type = Type.GetType(modelName);
                if (type != null && type.GetProperty("name") != null)
                {
                    codeBuilder.AppendLine($"\t\t\t\t}},Utility.CreateDynamicObject(paramList)).OrderBy(o => o.name).ToList();");
                }
                else
                {
                    codeBuilder.AppendLine($"\t\t\t\t}},Utility.CreateDynamicObject(paramList)).ToList();");
                }
                codeBuilder.AppendLine("\t\t\t}");
            }
            codeBuilder.AppendLine("\t\t\telse");
            codeBuilder.AppendLine("\t\t\t{");
            codeBuilder.AppendLine("\t\t\t\tquery = \"\";");


            type = Type.GetType(modelName);
            if (type != null && type.GetProperty("name") != null)
            {
                codeBuilder.AppendLine($"\t\t\treturn db.Query<{modelName}>(query).OrderBy(o => o.name).ToList();");
            }
            else
            {
                codeBuilder.AppendLine($"\t\t\treturn db.Query<{modelName}>(query).ToList();");
            }
            codeBuilder.AppendLine("\t\t\t}");

            codeBuilder.AppendLine("\t\t}");
            codeBuilder.AppendLine("\t}");

            // GetById method
            codeBuilder.AppendLine($"\tpublic static {modelName} GetById(string id, Utility.FillStyle fillStyle = Utility.FillStyle.Basic)");
            codeBuilder.AppendLine("\t{");
            codeBuilder.AppendLine("\t\tusing (IDbConnection db = new SQLiteConnection(Utility.ConnString))");
            codeBuilder.AppendLine("\t\t{");
            codeBuilder.AppendLine("\t\t\tdb.Open();");
            codeBuilder.AppendLine("\t\t\tstring query = \"\";");
            codeBuilder.AppendLine("\t\t\tif (fillStyle == Utility.FillStyle.Basic)");
            codeBuilder.AppendLine("\t\t\t{");
            codeBuilder.AppendLine($"\t\t\t\tquery = \"{Utility.SelectQuery(fields.FirstOrDefault()?.tableNav?.id ?? "", Utility.FillStyle.Basic) + " WHERE id = @id"}\";");
            codeBuilder.AppendLine($"\t\t\treturn db.Query<{modelName}>(query,new {{id=id}}).FirstOrDefault()??new {modelName}();");
            codeBuilder.AppendLine("\t\t\t}");
            codeBuilder.AppendLine("\t\t\telse if (fillStyle == Utility.FillStyle.AllProperties)");
            codeBuilder.AppendLine("\t\t\t{");
            codeBuilder.AppendLine($"\t\t\t\tquery = \"{Utility.SelectQuery(fields.FirstOrDefault()?.tableNav?.id ?? "", Utility.FillStyle.AllProperties) + " WHERE id = @id"}\";");
            codeBuilder.AppendLine($"\t\t\treturn db.Query<{modelName}>(query,new {{id=id}}).FirstOrDefault()??new {modelName}();");
            codeBuilder.AppendLine("\t\t\t}");
            if (fields.Where(fld => !string.IsNullOrEmpty(fld.foreignKeyId)).Count() > 0)
            {
                codeBuilder.AppendLine("\t\t\telse if (fillStyle == Utility.FillStyle.WithBasicNav)");
                codeBuilder.AppendLine("\t\t\t{");
                codeBuilder.AppendLine($"\t\t\t\tquery = \"{Utility.SelectQuery(fields.FirstOrDefault()?.tableNav?.id ?? "", Utility.FillStyle.WithBasicNav) + " WHERE myBaseTable.id = @id"}\";");
                codeBuilder.Append($"\t\t\t\treturn db.Query<{modelName}");

                foreach (FeplField field in fields.Where(fld => !string.IsNullOrEmpty(fld.foreignKeyId)))
                {
                    codeBuilder.Append($", {field.foreignKeyNav.tableNav.modelName}");
                }
                codeBuilder.AppendLine($", {modelName}>(query,");

                List<string> tableList = new List<string>();
                tableList.Add("myBaseTable");
                int refNo = 1;
                foreach (FeplField field in fields.Where(fld => !string.IsNullOrEmpty(fld.foreignKeyId)))
                {
                    tableList.Add("ref" + refNo);
                    refNo++;
                }

                codeBuilder.AppendLine($"\t\t\t\t({string.Join(",", tableList)}) =>");
                codeBuilder.AppendLine($"\t\t\t\t{{");

                refNo = 1;
                foreach (FeplField field in fields.Where(fld => !string.IsNullOrEmpty(fld.foreignKeyId)))
                {
                    codeBuilder.AppendLine($"\t\t\t\t\tmyBaseTable.{field.name.TrimId()}Nav =ref{refNo} ;");
                    refNo++;
                }
                codeBuilder.AppendLine($"\t\t\t\t\treturn myBaseTable;");

                codeBuilder.AppendLine($"\t\t\t\t}} ,new {{id=id}} ).FirstOrDefault()??new {modelName}();");

                codeBuilder.AppendLine("\t\t\t}");
                codeBuilder.AppendLine("\t\t\telse if (fillStyle == Utility.FillStyle.WithFullNav)");
                codeBuilder.AppendLine("\t\t\t{");
                codeBuilder.AppendLine($"\t\t\t\tquery = \"{Utility.SelectQuery(fields.FirstOrDefault()?.tableNav?.id ?? "", Utility.FillStyle.WithFullNav) + " WHERE myBaseTable.id = @id"}\";");
                codeBuilder.Append($"\t\t\t\treturn db.Query<{modelName}");

                foreach (FeplField field in fields.Where(fld => !string.IsNullOrEmpty(fld.foreignKeyId)))
                {
                    codeBuilder.Append($", {field.foreignKeyNav.tableNav.modelName}");
                }
                codeBuilder.AppendLine($", {modelName}>(query,");

                tableList = new List<string>();
                tableList.Add("myBaseTable");
                refNo = 1;
                foreach (FeplField field in fields.Where(fld => !string.IsNullOrEmpty(fld.foreignKeyId)))
                {
                    tableList.Add("ref" + refNo);
                    refNo++;
                }

                codeBuilder.AppendLine($"\t\t\t\t({string.Join(",", tableList)}) =>");
                codeBuilder.AppendLine($"\t\t\t\t{{");

                refNo = 1;
                foreach (FeplField field in fields.Where(fld => !string.IsNullOrEmpty(fld.foreignKeyId)))
                {
                    codeBuilder.AppendLine($"\t\t\t\t\tmyBaseTable.{field.name.TrimId()}Nav =ref{refNo} ;");
                    refNo++;
                }
                codeBuilder.AppendLine($"\t\t\t\t\treturn myBaseTable;");

                codeBuilder.AppendLine($"\t\t\t\t}} ,new {{id=id}} ).FirstOrDefault()??new {modelName}();");

                codeBuilder.AppendLine("\t\t\t}");
            }
            codeBuilder.AppendLine("\t\t\telse");
            codeBuilder.AppendLine("\t\t\t{");
            codeBuilder.AppendLine("\t\t\t\tquery = \"\";");
            codeBuilder.AppendLine($"\t\t\treturn db.Query<{modelName}>(query).FirstOrDefault()??new {modelName}();");
            codeBuilder.AppendLine("\t\t\t}");

            codeBuilder.AppendLine("\t\t}");
            codeBuilder.AppendLine("\t}");

            // Save method


            string var = "";
            string deletevar = "";
            string deletevarparam = "";
            foreach (FeplField field in fields)
            {
                if (field.dataTypeId == "F6")
                {
                    var += "string  " + field.name;
                    var += ",";
                }
                else if (field.dataTypeId == "F7")
                {
                    var += " IFormFile  " + field.name + "File ";
                    var += ",";
                    deletevar += "string  " + field.name + "File";
                    deletevar += ",";
                    deletevarparam += field.name + "File ";
                    deletevarparam += ",";
                }
            }

            if (fields.Where(o => o.dataTypeId == "F7").Count() > 0)
            {
                var += "string  " + "path ";
                //var += ",";
                deletevar += " string  " + "path";
                //deletevar += ",";
            }
            if (var.Length > 0)
            {
                var = var.Remove(var.Length - 1);

            }
            if (deletevarparam.Length > 0)
            {
                deletevarparam = deletevarparam.Remove(deletevarparam.Length - 1);

            }



            codeBuilder.AppendLine($"\tpublic bool Save({var})");
            codeBuilder.AppendLine("\t{");
            codeBuilder.AppendLine("\t\tusing (IDbConnection db = new SQLiteConnection(Utility.ConnString))");
            codeBuilder.AppendLine("\t\t{");
            codeBuilder.AppendLine("\t\t\tdb.Open();");
            codeBuilder.AppendLine("\t\t\tusing (var transaction = db.BeginTransaction())");
            codeBuilder.AppendLine("\t\t\t{");
            codeBuilder.AppendLine("\t\t\t\ttry");
            codeBuilder.AppendLine("\t\t\t\t{");



            codeBuilder.AppendLine("\t\t\t\t\tidPrefix = \"F\";");
            codeBuilder.AppendLine($"\t\t\t\t\tnumId = db.ExecuteScalar<int>(\"{Utility.MaxIdQuery(fields.FirstOrDefault()?.tableNav?.id ?? "")}\") + 1;");
            codeBuilder.AppendLine("\t\t\t\t\tid = idPrefix + numId;");
            codeBuilder.AppendLine("\t\t\t\t\t string extension = \"\";");

            foreach (FeplField field in fields)
            {
                if (field.dataTypeId == "F6")
                {
                    codeBuilder.AppendLine($"\t\t\t\t\tthis.{field.name} = int.Parse(DateTime.Parse({field.name}).ToString(\"yyyyMMdd\"));");

                }
                else if (field.dataTypeId == "F7")
                {
                    codeBuilder.AppendLine($"\t\t\t\t\tif ({field.name + "File"} != null)");
                    codeBuilder.AppendLine("\t{");
                    codeBuilder.AppendLine($"\t\t\t\t\textension = Path.GetExtension({field.name + "File"}.FileName);");
                    codeBuilder.AppendLine("\t\t\t\t\tif (extension == \".pdf\" || extension == \".jpg\" || extension == \".jpeg\" || extension == \".png\")");
                    codeBuilder.AppendLine("\t{");
                    codeBuilder.AppendLine("\t\t\t\t\tstring uploadsFolder = Path.Combine(path, \"Image\");");
                    codeBuilder.AppendLine("\t\t\t\t\tif (!Directory.Exists(uploadsFolder))");
                    codeBuilder.AppendLine("\t\t\t\t\t{");
                    codeBuilder.AppendLine("\t\t\t\t\tDirectory.CreateDirectory(uploadsFolder);");
                    codeBuilder.AppendLine("\t\t\t\t\t}");
                    codeBuilder.AppendLine($"\t\t\t\t\tstring uniqueFileName = DateTime.Now.ToString(\"yyyyMMdd_HHmmssfff\") + \" - \" + {field.name + "File"}.FileName;");
                    codeBuilder.AppendLine($"\t\t\t\t\tthis.{field.name} = uniqueFileName;");
                    codeBuilder.AppendLine("\t\t\t\t\tstring filePath = Path.Combine(uploadsFolder, uniqueFileName);");
                    codeBuilder.AppendLine("\t\t\t\t\tusing (var stream = new FileStream(filePath, FileMode.Create))");
                    codeBuilder.AppendLine("\t{");
                    codeBuilder.AppendLine($"\t\t\t\t{field.name}File.CopyTo(stream);");

                    codeBuilder.AppendLine("\t}");
                    codeBuilder.AppendLine("\t}");
                    codeBuilder.AppendLine("\t}");
                }
            }



            codeBuilder.AppendLine($"\t\t\t\t\tstring sql = \"{Utility.InsertQuery(fields.FirstOrDefault()?.tableNav?.id ?? "")}\";");
            codeBuilder.AppendLine("\t\t\t\t\tint affectedRows = db.Execute(sql, this, transaction);");
            codeBuilder.AppendLine("\t\t\t\t\ttransaction.Commit();");
            codeBuilder.AppendLine("\t\t\t\t\treturn true;");
            codeBuilder.AppendLine("\t\t\t\t}");
            codeBuilder.AppendLine("\t\t\t\tcatch (Exception ex)");
            codeBuilder.AppendLine("\t\t\t\t{");
            codeBuilder.AppendLine("\t\t\t\t\ttransaction.Rollback();");
            codeBuilder.AppendLine("\t\t\t\t\treturn false;");
            codeBuilder.AppendLine("\t\t\t\t}");
            codeBuilder.AppendLine("\t\t\t}");
            codeBuilder.AppendLine("\t\t}");
            codeBuilder.AppendLine("\t}");

            // Update method
            codeBuilder.AppendLine($"\tpublic bool Update({var})");
            codeBuilder.AppendLine("\t{");
            codeBuilder.AppendLine("\t\tusing (IDbConnection db = new SQLiteConnection(Utility.ConnString))");
            codeBuilder.AppendLine("\t\t{");
            codeBuilder.AppendLine("\t\t\tdb.Open();");
            codeBuilder.AppendLine("\t\t\tusing (var transaction = db.BeginTransaction())");
            codeBuilder.AppendLine("\t\t\t{");
            codeBuilder.AppendLine("\t\t\t\ttry");
            codeBuilder.AppendLine("\t\t\t\t{");
            codeBuilder.AppendLine("\t\t\t\t\t string extension = \"\";");
            int i = 0;
            foreach (FeplField field in fields)
            {
                if (field.dataTypeId == "F6")
                {
                    codeBuilder.AppendLine($"\t\t\t\t\tthis.{field.name} = int.Parse(DateTime.Parse({field.name}).ToString(\"yyyyMMdd\"));");

                }
                else if (field.dataTypeId == "F7")
                {
                    codeBuilder.AppendLine($"\t\t\t\t\tif ({field.name + "File"} != null)");
                    codeBuilder.AppendLine("\t{");
                    codeBuilder.AppendLine($"\t\t\t\t\textension = Path.GetExtension({field.name + "File"}.FileName);");
                    codeBuilder.AppendLine($"\t\t\t\t\tif (extension == \".pdf\" || extension == \".jpg\" || extension == \".jpeg\" || extension == \".png\")");
                    codeBuilder.AppendLine("\t{");
                    codeBuilder.AppendLine($"\t\t\t\t\tstring filename{i} = (path + \"/Image/\" + this.{field.name}).Replace('/','\\\\');");
                    codeBuilder.AppendLine($"\t\t\t\t\tFileInfo FL{i} = new FileInfo(filename{i});");
                    codeBuilder.AppendLine($"\t\t\t\t\tif (FL{i}.Exists)");
                    codeBuilder.AppendLine("\t{");
                    codeBuilder.AppendLine($"\t\t\t\t\tFL{i}.Delete();");
                    codeBuilder.AppendLine("\t}");
                    codeBuilder.AppendLine($"\t\t\t\t\tstring uploadsFolder = Path.Combine(path, \"Image\");");
                    codeBuilder.AppendLine($"\t\t\t\t\tstring uniqueFileName = DateTime.Now.ToString(\"yyyyMMdd_HHmmssfff\") + \" - \" + {field.name + "File"}.FileName;");
                    codeBuilder.AppendLine($"\t\t\t\t\tthis.{field.name} = uniqueFileName;");
                    codeBuilder.AppendLine($"\t\t\t\t\tstring filePath = Path.Combine(uploadsFolder, uniqueFileName);");
                    codeBuilder.AppendLine($"\t\t\t\t\tusing (var stream = new FileStream(filePath, FileMode.Create))");
                    codeBuilder.AppendLine("\t{");
                    codeBuilder.AppendLine($"\t\t\t\t{field.name}File.CopyTo(stream);");

                    codeBuilder.AppendLine("\t}");
                    codeBuilder.AppendLine("\t}");
                    codeBuilder.AppendLine("\t}");
                    i++;
                }
            }

            codeBuilder.AppendLine($"\t\t\t\t\tstring sql = \"{Utility.UpdateQuery(fields.FirstOrDefault()?.tableNav?.id ?? "")}\";");
            codeBuilder.AppendLine("\t\t\t\t\tint affectedRows = db.Execute(sql, this, transaction);");
            codeBuilder.AppendLine("\t\t\t\t\ttransaction.Commit();");
            codeBuilder.AppendLine("\t\t\t\t\treturn true;");
            codeBuilder.AppendLine("\t\t\t\t}");
            codeBuilder.AppendLine("\t\t\t\tcatch (Exception ex)");
            codeBuilder.AppendLine("\t\t\t\t{");
            codeBuilder.AppendLine("\t\t\t\t\ttransaction.Rollback();");
            codeBuilder.AppendLine("\t\t\t\t\treturn false;");
            codeBuilder.AppendLine("\t\t\t\t}");
            codeBuilder.AppendLine("\t\t\t}");
            codeBuilder.AppendLine("\t\t}");
            codeBuilder.AppendLine("\t}");

            // Delete method
            codeBuilder.AppendLine($"\tpublic string Delete({deletevar})");
            codeBuilder.AppendLine("\t{");
            codeBuilder.AppendLine("\t\tusing (IDbConnection db = new SQLiteConnection(Utility.ConnString))");
            codeBuilder.AppendLine("\t\t{");
            codeBuilder.AppendLine("\t\t\tdb.Open();");
            codeBuilder.AppendLine("\t\t\tusing (var transaction = db.BeginTransaction())");
            codeBuilder.AppendLine("\t\t\t{");
            codeBuilder.AppendLine("\t\t\t\ttry");
            codeBuilder.AppendLine("\t\t\t\t{");
            i = 0;
            foreach (FeplField field in fields)
            {
                if (field.dataTypeId == "F7")
                {
                    codeBuilder.AppendLine($"\t\t\t\t\t string filename{i} = (path + \"/Image/\" + {field.name + "File"}).Replace('/','\\\\');");
                    codeBuilder.AppendLine($"\t\t\t\t\tFileInfo FL{i} = new FileInfo(filename{i});");
                    codeBuilder.AppendLine($"\t\t\t\t\tif (FL{i}.Exists)");
                    codeBuilder.AppendLine("\t{");
                    codeBuilder.AppendLine($"\t\t\t\t\tFL{i}.Delete();");
                    codeBuilder.AppendLine("\t}");
                    i++;
                }
            }
            codeBuilder.AppendLine($"\t\t\t\t\tstring sql = \"{Utility.DeleteQuery(fields.FirstOrDefault()?.tableNav?.id ?? "")}\";");
            codeBuilder.AppendLine("\t\t\t\t\tint affectedRows = db.Execute(sql, new { id = this.id }, transaction);");
            codeBuilder.AppendLine("\t\t\t\t\ttransaction.Commit();");
            codeBuilder.AppendLine("\t\t\t\t\treturn \"true\";");
            codeBuilder.AppendLine("\t\t\t\t}");
            codeBuilder.AppendLine("\t\t\t\tcatch (Exception ex)");
            codeBuilder.AppendLine("\t\t\t\t{");
            codeBuilder.AppendLine("\t\t\t\t\ttransaction.Rollback();");
            codeBuilder.AppendLine("\t\t\t\t\treturn ex.Message;");
            codeBuilder.AppendLine("\t\t\t\t}");
            codeBuilder.AppendLine("\t\t\t}");
            codeBuilder.AppendLine("\t\t}");
            codeBuilder.AppendLine("\t}");

            // Close the model class
            codeBuilder.AppendLine("}");
            codeBuilder.AppendLine("}");

            return codeBuilder.ToString();
        }
        public static string GenerateControllerCode(List<FeplField> fields, string modelName, string tableName, FeplProject project)
        {
            StringBuilder codeBuilder = new StringBuilder();

            // Generate controller class header
            codeBuilder.AppendLine("using System;");
            codeBuilder.AppendLine("using System.Collections.Generic;");
            codeBuilder.AppendLine("using Microsoft.AspNetCore.Authorization;");
            codeBuilder.AppendLine("using Microsoft.AspNetCore.Mvc;");
            codeBuilder.AppendLine("using ClosedXML.Excel;");
            codeBuilder.AppendLine("using iTextSharp.text;");
            codeBuilder.AppendLine("using iTextSharp.text.pdf;");
            codeBuilder.AppendLine("using Newtonsoft.Json;");
            codeBuilder.AppendLine("using System.Data;");
            codeBuilder.AppendLine("using System.Data.SQLite;");
            codeBuilder.AppendLine("using Dapper;");
            codeBuilder.AppendLine($"using {project.name}.Models;");
            codeBuilder.AppendLine();
            codeBuilder.AppendLine($"namespace {project.name}.Controllers");
            codeBuilder.AppendLine("{");

            //if (project.isLoginRequired == 1)
            //{
            //    codeBuilder.AppendLine("[Authorize]");
            //}

            codeBuilder.AppendLine("[AllowAnonymous]");
            codeBuilder.AppendLine($"public class {modelName}Controller : Controller");

            codeBuilder.AppendLine("{");

            // constructor Code
            codeBuilder.AppendLine("\tprivate IConfiguration configuration;");
            codeBuilder.AppendLine("\tprivate readonly IWebHostEnvironment webHost;");
            codeBuilder.AppendLine($"\tprivate readonly ILogger<{modelName}Controller> _logger;");
            codeBuilder.AppendLine();

            codeBuilder.AppendLine($"\tpublic {modelName}Controller(IConfiguration _configuration, ILogger<{modelName}Controller> logger, IWebHostEnvironment _webHost)");
            codeBuilder.AppendLine("\t{");
            codeBuilder.AppendLine("\t\tconfiguration = _configuration;");
            codeBuilder.AppendLine("\t\t_logger = logger;");
            codeBuilder.AppendLine("\t\twebHost = _webHost;");
            codeBuilder.AppendLine("\t}");

            //  Generate List method get
            codeBuilder.AppendLine("\t// List");
            codeBuilder.AppendLine("\t[HttpGet]");

            var insertColumns = string.Join(", string ", fields.Where(o => !string.IsNullOrEmpty(o.foreignKeyId)).Select(f => f.name));
            if (!string.IsNullOrEmpty(insertColumns))
            {
                insertColumns = "string " + insertColumns;
            }

            codeBuilder.AppendLine($"\tpublic IActionResult Index({insertColumns})");
            codeBuilder.AppendLine("\t{");

            codeBuilder.AppendLine("\t\ttry");
            codeBuilder.AppendLine("\t\t{");

            foreach (FeplField field in fields.Where(fld => !string.IsNullOrEmpty(fld.foreignKeyId)))
            {
                codeBuilder.AppendLine($"\t\t\t\tif (!string.IsNullOrEmpty({field.name}))");
                codeBuilder.AppendLine("\t\t\t{");
                codeBuilder.AppendLine($"\t\t\t\tViewBag.{field.name} = {field.name};");
                codeBuilder.AppendLine("\t\t\t}");
            }

            //codeBuilder.AppendLine("");

            foreach (FeplField field in fields.Where(fld => !string.IsNullOrEmpty(fld.foreignKeyId)))
            {
                codeBuilder.AppendLine($"\t\t\t\tViewBag.{field.foreignKeyNav?.tableNav?.name} = {field.foreignKeyNav?.tableNav?.modelName}.Get(Utility.FillStyle.Basic);");
            }

            //codeBuilder.AppendLine("");
            codeBuilder.AppendLine("Dictionary<string,string> param = new Dictionary<string, string>();");
            foreach (FeplField field in fields.Where(fld => !string.IsNullOrEmpty(fld.foreignKeyId)))
            {
                codeBuilder.AppendLine($"param.Add(\"{field.name}\", {field.name});");
            }

            if (fields.Where(fld => !string.IsNullOrEmpty(fld.foreignKeyId)).Count() > 0)
            {
                Type type = Type.GetType(modelName);
                if (type != null && type.GetProperty("name") != null)
                {
                    codeBuilder.AppendLine($"\t\t\t\tList<{modelName}> records = {modelName}.Get(Utility.FillStyle.WithBasicNav, param).OrderBy(o => o.name).ToList();");
                }
                else
                {
                    codeBuilder.AppendLine($"\t\t\t\tList<{modelName}> records = {modelName}.Get(Utility.FillStyle.WithBasicNav, param).ToList();");
                }
            }
            else
            {
                Type type = Type.GetType(modelName);
                if (type != null && type.GetProperty("name") != null)
                {
                    codeBuilder.AppendLine($"\t\t\t\tList<{modelName}> records = {modelName}.Get(Utility.FillStyle.AllProperties, param).OrderBy(o => o.name).ToList();");
                }
                else
                {
                    codeBuilder.AppendLine($"\t\t\t\tList<{modelName}> records = {modelName}.Get(Utility.FillStyle.AllProperties, param).ToList();");
                }
            }


            codeBuilder.AppendLine("\t\t\t\treturn View(records);");
            codeBuilder.AppendLine("\t\t\t}");
            codeBuilder.AppendLine("\t\tcatch (Exception ex)");
            codeBuilder.AppendLine("\t\t{");
            codeBuilder.AppendLine("\t\t\t// Log the exception or handle it as needed");
            codeBuilder.AppendLine("\t\t\treturn StatusCode(500, \"An error occurred while retrieving data from the database.\");");
            codeBuilder.AppendLine("\t\t}");
            codeBuilder.AppendLine("\t}");

            //  Generate List method post
            codeBuilder.AppendLine("\t// List");
            codeBuilder.AppendLine("\t[HttpPost]");
            insertColumns = string.Join(", string ", fields.Where(o => !string.IsNullOrEmpty(o.foreignKeyId)).Select(f => f.name));
            if (!string.IsNullOrEmpty(insertColumns))
            {
                insertColumns = ", string " + insertColumns;
            }
            List<string> param = new List<string>();
            foreach (FeplField feplField in fields.Where(o => !string.IsNullOrEmpty(o.foreignKeyId)))
            {
                param.Add(feplField.name + " = " + feplField.name);
            }
            codeBuilder.AppendLine($"\tpublic IActionResult Index(string id {insertColumns})");
            codeBuilder.AppendLine("\t{");
            if (param.Count > 0)
            {
                codeBuilder.AppendLine($"\t\t\t\t return RedirectToAction(\"Index\", new {{ {string.Join(", ", param)} }});");
            }
            else
            {
                codeBuilder.AppendLine("\t\t\t\t return RedirectToAction(\"Index\");");

            }
            codeBuilder.AppendLine("\t}");

            // Generate Create (GET) method

            codeBuilder.AppendLine("\t// GET: Display form to create a new record");
            codeBuilder.AppendLine("\t[HttpGet]");
            codeBuilder.AppendLine($"\tpublic IActionResult Create(string url)");
            codeBuilder.AppendLine("\t{");
            codeBuilder.AppendLine("\t\ttry");
            codeBuilder.AppendLine("\t\t{");
            codeBuilder.AppendLine("");
            codeBuilder.AppendLine("\t\t\t\tViewBag.url= url;");

            foreach (FeplField field in fields.Where(fld => !string.IsNullOrEmpty(fld.foreignKeyId)))
            {
                codeBuilder.AppendLine($"\t\t\t\tViewBag.{field.foreignKeyNav?.tableNav?.name} = {field.foreignKeyNav?.tableNav?.modelName}.Get(Utility.FillStyle.AllProperties);");
            }

            codeBuilder.AppendLine($"\t\t\t\treturn View(new {modelName}());");
            codeBuilder.AppendLine("\t\t\t}");
            codeBuilder.AppendLine("\t\tcatch (Exception ex)");
            codeBuilder.AppendLine("\t\t{");
            codeBuilder.AppendLine("\t\t\t// Log the exception or handle it as needed");
            codeBuilder.AppendLine("\t\t\treturn StatusCode(500, \"An error occurred while retrieving data from the database.\");");
            codeBuilder.AppendLine("\t\t}");
            codeBuilder.AppendLine("\t}");


            // Generate Create (POST) method
            codeBuilder.AppendLine("\t// POST: Create a new record");
            codeBuilder.AppendLine("\t[HttpPost]");




            string var = "";
            foreach (FeplField field in fields)
            {
                if (field.dataTypeId == "F6")
                {
                    var += " , string  " + field.name + " ";
                }
                if (field.dataTypeId == "F7")
                {
                    var += " , IFormFile  " + field.name + "File ";
                }
            }
            if (var.Length > 0)
            {
                var = var.Remove(var.Length - 1);
            }
            if (fields.Where(o => o.metaRequired == 1).Count() > 0)
            {
                codeBuilder.AppendLine($"\tpublic IActionResult Create({modelName} model, string url{var}, string content)");
            }
            else
            {
                codeBuilder.AppendLine($"\tpublic IActionResult Create({modelName} model, string url{var})");
            }
            codeBuilder.AppendLine("\t{");
            if (fields.Where(o => o.metaRequired == 1).Count() > 0)
            {
                codeBuilder.AppendLine($"model = JsonConvert.DeserializeObject<{modelName}>(content);");
            }
            string var2 = "";
            foreach (FeplField field in fields)
            {
                if (field.dataTypeId == "F6")
                {
                    var2 += "" + field.name;
                    var2 += ",";
                }
                if (field.dataTypeId == "F7")
                {
                    var2 += "" + field.name + "File ,";
                }
            }
            if (var2.Length > 0)
            {
                var2 = var2.Remove(var2.Length - 1);
            }
            if (fields.Where(o => o.dataTypeId == "F7").Count() > 0)
            {
                codeBuilder.AppendLine("\tstring path = webHost.WebRootPath;");
                var2 += " , path ";
            }

            List<FeplField> fieldm = FeplField.Get(Utility.FillStyle.WithFullNav);
            if (fields.Where(o => o.metaRequired == 1).Count() > 0)
            {
                string fieldname = fields.Where(o => o.metaRequired == 1).FirstOrDefault().printName.ToLower();
                //string tablename = fields.Where(o => o.metaTableId == o.id).FirstOrDefault().name;
                string tablename = fields.Where(o => o.metaRequired == 1).FirstOrDefault().printName;
                codeBuilder.AppendLine("\t\tusing (SQLiteConnection db = new SQLiteConnection(Utility.ConnString))");
                codeBuilder.AppendLine("\t\t{");
                codeBuilder.AppendLine("\t\t   db.Open();");
                codeBuilder.AppendLine("\t\t\tusing (var transaction = db.BeginTransaction())");
                codeBuilder.AppendLine("\t\t\t{");
                codeBuilder.AppendLine("\t\t\t\ttry");
                codeBuilder.AppendLine("\t\t\t\t{");
                codeBuilder.AppendLine("\t\t\t\t\tmodel.idPrefix = \"F\";");
                codeBuilder.AppendLine($"\t\t\t\t\tmodel.numId = db.ExecuteScalar<int>(\"{Utility.MaxIdQuery(fields.FirstOrDefault()?.tableNav?.id ?? "")}\") + 1;");
                codeBuilder.AppendLine("\t\t\t\t\tmodel.id = model.idPrefix + model.numId;");
                codeBuilder.AppendLine($"\t\t\t\t\tstring sql = \"{Utility.InsertQuery(fields.FirstOrDefault()?.tableNav?.id ?? "")}\";");
                codeBuilder.AppendLine($"\t\t\t\t\tint affectedRows = db.Execute(sql, model, transaction);");
                codeBuilder.AppendLine($"\t\tforeach ({tablename} {tablename.ToLower()} in model.{fieldname})");
                codeBuilder.AppendLine("\t\t {");
                codeBuilder.AppendLine($"\t\t{tablename.ToLower()}.numId = db.ExecuteScalar<int>(\"select max(numId) from {fieldm.Where(o => o.tableId == fields.Where(o => o.metaRequired == 1).FirstOrDefault().metaTableId).FirstOrDefault()?.tableNav.name}\") + 1;");
                codeBuilder.AppendLine($"\t\t{tablename.ToLower()}.idPrefix = \"F\";");
                codeBuilder.AppendLine($"\t\t{tablename.ToLower()}.id = {tablename.ToLower()}.idPrefix+{tablename.ToLower()}.numId;");
                codeBuilder.AppendLine($"\t\t{tablename.ToLower()}.{modelName.ToLower()}Id = model.id;");


                //codeBuilder.AppendLine($"\t\tdb.Execute(\"insert into {tablename}() values()\", {tablename.ToLower()});");
                codeBuilder.AppendLine($"\t\tdb.Execute(\"{Utility.InsertQuery(fields.Where(a => a.metaRequired == 1).FirstOrDefault().metaTableId ?? "")}\",{tablename.ToLower()});");
                codeBuilder.AppendLine("\t\t}");
                codeBuilder.AppendLine("\t\t\t\t\ttransaction.Commit();");
                codeBuilder.AppendLine("\t\treturn Json(new { status = \"ok\" });");
                codeBuilder.AppendLine("\t\t\t\t}");
                codeBuilder.AppendLine("\t\t\t\tcatch (Exception ex)");
                codeBuilder.AppendLine("\t\t\t\t{");
                codeBuilder.AppendLine("\t\t\t\t\ttransaction.Rollback();");
                codeBuilder.AppendLine("\t\t\t\t\tViewBag.error = ex.Message;");
                codeBuilder.AppendLine("\t\t\t\t\treturn View(model);");
                codeBuilder.AppendLine("\t\t\t\t}");
                codeBuilder.AppendLine("\t\t\t}");
                codeBuilder.AppendLine("\t\t}");
            }
            else
            {
                codeBuilder.AppendLine($"\t\t if (model.Save({var2}))");
                codeBuilder.AppendLine("\t\t{");
                codeBuilder.AppendLine("\t\t\treturn Redirect(url);");
                codeBuilder.AppendLine("\t\t}");
                codeBuilder.AppendLine("\t\telse");
                codeBuilder.AppendLine("\t\t{");
                codeBuilder.AppendLine("\t\t\treturn StatusCode(500, \"An error occurred while creating a new record.\");");
                codeBuilder.AppendLine("\t\t}");
            }

            codeBuilder.AppendLine("\t}");


            // Generate Edit (GET) method

            codeBuilder.AppendLine("\t[HttpGet]");
            codeBuilder.AppendLine($"\tpublic IActionResult Edit(string id, string url)");
            codeBuilder.AppendLine("\t{");
            if (fields.Where(fld => !string.IsNullOrEmpty(fld.foreignKeyId)).Count() > 0)
            {
                codeBuilder.AppendLine($"\t{modelName} model = {modelName}.GetById(id,Utility.FillStyle.WithBasicNav);");
            }
            else
            {
                codeBuilder.AppendLine($"\t{modelName} model = {modelName}.GetById(id,Utility.FillStyle.AllProperties);");
            }
            if (fields.Where(o => o.metaRequired == 1).Count() > 0)
            {
                codeBuilder.AppendLine("\t\tusing (SQLiteConnection db = new SQLiteConnection(Utility.ConnString))");
                codeBuilder.AppendLine("\t\t{");
                codeBuilder.AppendLine("\t\t db.Open();");
                //codeBuilder.AppendLine($"\tmodel.{fields.Where(o => o.metaRequired == 1).FirstOrDefault().printName.ToLower()} = {fields.Where(o => o.metaRequired == 1).FirstOrDefault().printName}.GetById(id,Utility.FillStyle.WithBasicNav);");
                codeBuilder.AppendLine($"\tmodel.{fields.Where(o => o.metaRequired == 1).FirstOrDefault().printName.ToLower()} = db.Query<{fields.Where(o => o.metaRequired == 1).FirstOrDefault().printName}>(\"select * from  {fieldm.Where(o => o.tableId == fields.Where(o => o.metaRequired == 1).FirstOrDefault().metaTableId).FirstOrDefault()?.tableNav.name} where {modelName.ToLower()}Id = '\" + model.id+\"'\").ToList();");
                codeBuilder.AppendLine("\t\t}");
            }
            codeBuilder.AppendLine("\t\t\t\tViewBag.url = url;");

            foreach (FeplField field in fields.Where(fld => !string.IsNullOrEmpty(fld.foreignKeyId)))
            {
                codeBuilder.AppendLine($"\t\t\t\tViewBag.{field.foreignKeyNav?.tableNav?.name} = {field.foreignKeyNav?.tableNav?.modelName}.Get(Utility.FillStyle.AllProperties);");
            }

            codeBuilder.AppendLine("\tif (model == null)");
            codeBuilder.AppendLine("\t\t{");
            codeBuilder.AppendLine("\t\treturn NotFound(); // Return 404 Not Found if the record is not found");
            codeBuilder.AppendLine("\t\t}");
            codeBuilder.AppendLine("\t\treturn View(model);");
            codeBuilder.AppendLine("\t}");


            // Generate Edit (POST) method

            codeBuilder.AppendLine("\t// POST: Create a new record");
            codeBuilder.AppendLine("\t[HttpPost]");
            if (fields.Where(o => o.metaRequired == 1).Count() > 0)
            {
                codeBuilder.AppendLine($"\tpublic IActionResult Edit({modelName} model, string url{var}, string content)");
            }
            else
            {
                codeBuilder.AppendLine($"\tpublic IActionResult Edit({modelName} model, string url{var})");
            }
            codeBuilder.AppendLine("\t{");
            if (fields.Where(o => o.dataTypeId == "F7").Count() > 0)
            {
                codeBuilder.AppendLine("\tstring path = webHost.WebRootPath;");
            }
            if (fields.Where(o => o.metaRequired == 1).Count() > 0)
            {
                codeBuilder.AppendLine($"model = JsonConvert.DeserializeObject<{modelName}>(content);");
            }

            if (fields.Where(o => o.metaRequired == 1).Count() > 0)
            {
                string fieldname = fields.Where(o => o.metaRequired == 1).FirstOrDefault().printName.ToLower();
                string tablename = fields.Where(o => o.metaRequired == 1).FirstOrDefault().printName;
                codeBuilder.AppendLine("\t\tusing (SQLiteConnection db = new SQLiteConnection(Utility.ConnString))");
                codeBuilder.AppendLine("\t\t{");
                codeBuilder.AppendLine("\t\t   db.Open();");
                codeBuilder.AppendLine("\t\t\tusing (var transaction = db.BeginTransaction())");
                codeBuilder.AppendLine("\t\t\t{");
                codeBuilder.AppendLine("\t\t\t\ttry");
                codeBuilder.AppendLine("\t\t\t\t{");
                string vardecl = "";
                foreach (FeplField field in fields)
                {
                    if (field.name == "id" || field.name == "numId" || field.name == "idPrefix")
                    {

                    }
                    else
                    {
                        vardecl += "" + field.name + "=@" + field.name + " ,";
                    }
                }
                vardecl = vardecl.Remove(vardecl.Length - 1);
                codeBuilder.AppendLine($"\t\t\t\t\tstring sql = \"update {tableName} set {vardecl} where id = @id\";");

                codeBuilder.AppendLine($"\t\t\t\t\tint affectedRows = db.Execute(sql, model, transaction);");

                codeBuilder.AppendLine($" db.Execute(\"DELETE FROM {fieldm.Where(o => o.tableId == fields.Where(o => o.metaRequired == 1).FirstOrDefault()?.metaTableId).FirstOrDefault()?.tableNav.name} WHERE {modelName.ToLower()}Id = '\"+ model.id+\"'\");");
                codeBuilder.AppendLine($"\t\tforeach ({tablename} {tablename.ToLower()} in model.{fieldname})");
                codeBuilder.AppendLine("\t\t {");
                codeBuilder.AppendLine($"\t\t{tablename.ToLower()}.numId = db.ExecuteScalar<int>(\"select max(numId) from {fieldm.Where(o => o.tableId == fields.Where(o => o.metaRequired == 1).FirstOrDefault()?.metaTableId).FirstOrDefault()?.tableNav.name}\") + 1;");
                codeBuilder.AppendLine($"\t\t{tablename.ToLower()}.idPrefix = \"F\";");
                codeBuilder.AppendLine($"\t\t{tablename.ToLower()}.id = {tablename.ToLower()}.idPrefix+{tablename.ToLower()}.numId;");
                codeBuilder.AppendLine($"\t\t{tablename.ToLower()}.{modelName.ToLower()}Id = model.id;");

                codeBuilder.AppendLine($"\t\tdb.Execute(\"{Utility.InsertQuery(fields.Where(a => a.metaRequired == 1).FirstOrDefault()?.metaTableId ?? "")}\",{tablename.ToLower()});");
                codeBuilder.AppendLine("\t\t}");
                codeBuilder.AppendLine("\t\t\t\t\ttransaction.Commit();");
                codeBuilder.AppendLine("\t\t return Json (new { status = \"ok\" });");
                codeBuilder.AppendLine("\t\t\t}");
                codeBuilder.AppendLine("\t\t\t\tcatch (Exception ex)");
                codeBuilder.AppendLine("\t\t\t\t{");
                codeBuilder.AppendLine("\t\t\t\t\ttransaction.Rollback();");
                codeBuilder.AppendLine("\t\t\t\t\tViewBag.error = ex.Message;");
                codeBuilder.AppendLine("\t\t\t\t\treturn View(model);");
                codeBuilder.AppendLine("\t\t\t\t}");
                codeBuilder.AppendLine("\t\t\t}");
                codeBuilder.AppendLine("\t\t}");
            }
            else
            {
                codeBuilder.AppendLine($"\t\t if (model.Update({var2}))");
                codeBuilder.AppendLine("\t\t{");
                codeBuilder.AppendLine("\t\t\treturn Redirect(url);");
                codeBuilder.AppendLine("\t\t}");
                codeBuilder.AppendLine("\t\telse");
                codeBuilder.AppendLine("\t\t{");
                codeBuilder.AppendLine("\t\t\treturn StatusCode(500, \"An error occurred while updating a new record.\");");
                codeBuilder.AppendLine("\t\t}");
            }


            codeBuilder.AppendLine("\t}");

            // Generate Delete (GET) method

            codeBuilder.AppendLine("\t[HttpGet]");
            codeBuilder.AppendLine($"\tpublic IActionResult Delete(string id, string url)");
            codeBuilder.AppendLine("\t{");
            if (fields.Where(fld => !string.IsNullOrEmpty(fld.foreignKeyId)).Count() > 0)
            {
                codeBuilder.AppendLine($"\t{modelName} model = {modelName}.GetById(id,Utility.FillStyle.WithBasicNav);");
            }
            else
            {
                codeBuilder.AppendLine($"\t{modelName} model = {modelName}.GetById(id,Utility.FillStyle.AllProperties);");
            }
            codeBuilder.AppendLine("\tViewBag.url = url;");
            codeBuilder.AppendLine("\tif (model == null)");
            codeBuilder.AppendLine("\t\t{");
            codeBuilder.AppendLine("\t\treturn NotFound(); // Return 404 Not Found if the record is not found");
            codeBuilder.AppendLine("\t\t}");
            codeBuilder.AppendLine("\t\treturn View(model);");
            codeBuilder.AppendLine("\t}");

            string var3 = "";
            string var4 = "";
            foreach (FeplField field in fields)
            {
                if (field.dataTypeId == "F7")
                {
                    var3 += " , string  " + field.name + "File ";
                    //var4 += " , " + field.name + "File ";
                    var4 += field.name + "File ,";
                }
            }
            if (fields.Where(o => o.dataTypeId == "F7").Count() > 0)
            {
                //var4 += " , path ";
                var4 += " path ";
            }
            // Generate Delete (POST) method


            codeBuilder.AppendLine("\t// POST: Create a new record");
            codeBuilder.AppendLine("\t[HttpPost]");
            codeBuilder.AppendLine($"\tpublic IActionResult Delete({modelName} model,string url{var3})");
            codeBuilder.AppendLine("\t{");
            if (fields.Where(o => o.dataTypeId == "F7").Count() > 0)
            {
                codeBuilder.AppendLine("\tstring path = webHost.WebRootPath;");
            }
            if (fields.Where(o => o.metaRequired == 1).Count() > 0)
            {
                string fieldname = fields.Where(o => o.metaRequired == 1).FirstOrDefault().printName.ToLower();
                string tablename = fields.Where(o => o.metaRequired == 1).FirstOrDefault().printName;

                codeBuilder.AppendLine("\t\tusing (IDbConnection db = new SQLiteConnection(Utility.ConnString))");
                codeBuilder.AppendLine("\t\t{");
                codeBuilder.AppendLine("\t\t\tdb.Open();");
                codeBuilder.AppendLine("\t\t\tusing (var transaction = db.BeginTransaction())");
                codeBuilder.AppendLine("\t\t\t{");
                codeBuilder.AppendLine("\t\t\t\ttry");
                codeBuilder.AppendLine("\t\t\t\t{");
                codeBuilder.AppendLine($"\t\t\t\t\tstring sql = \"{Utility.DeleteQuery(fields.FirstOrDefault()?.tableNav?.id ?? "")}\";");
                codeBuilder.AppendLine("\t\t\t\t\tint affectedRows = db.Execute(sql, new { id = model.id }, transaction);");
                codeBuilder.AppendLine($" db.Execute(\"DELETE FROM {fieldm.Where(o => o.tableId == fields.Where(o => o.metaRequired == 1).FirstOrDefault()?.metaTableId).FirstOrDefault()?.tableNav.name} WHERE {modelName.ToLower()}Id = '\"+ model.id+\"'\",\"\",transaction);");
                codeBuilder.AppendLine("\t\t\t\t\ttransaction.Commit();");

                codeBuilder.AppendLine("\t\t\treturn Redirect(url);");
                codeBuilder.AppendLine("\t\t}");
                codeBuilder.AppendLine("\t\tcatch(Exception ex)");
                codeBuilder.AppendLine("\t\t{");
                codeBuilder.AppendLine("\t\t\tViewBag.error = ex.Message;");
                codeBuilder.AppendLine("\t\t\treturn View(model);");
                codeBuilder.AppendLine("\t\t}");
                codeBuilder.AppendLine("\t}");
                codeBuilder.AppendLine("\t}");

            }
            else
            {
                codeBuilder.AppendLine($"\t\t string result = model.Delete({var4});");
                codeBuilder.AppendLine($"\t\t if (result == \"true\")");
                codeBuilder.AppendLine("\t\t{");
                codeBuilder.AppendLine("\t\t\treturn Redirect(url);");
                codeBuilder.AppendLine("\t\t}");
                codeBuilder.AppendLine("\t\telse");
                codeBuilder.AppendLine("\t\t{");
                codeBuilder.AppendLine("\t\t\tViewBag.error = result;");
                codeBuilder.AppendLine("\t\t\treturn View(model);");
                //codeBuilder.AppendLine("\t\t\treturn StatusCode(500, \"An error occurred while deleting a new record.\");");
                codeBuilder.AppendLine("\t\t}");
            }



            codeBuilder.AppendLine("\t}");



            //Generate EXCEL
            codeBuilder.AppendLine($"public FileContentResult GenerateExcel(string id {insertColumns})");

            codeBuilder.AppendLine("{");


            //codeBuilder.AppendLine($"List<{modelName}> model = new List<{modelName}>();");
            //codeBuilder.AppendLine("using (IDbConnection db = new SQLiteConnection(Utility.ConnString))");
            //codeBuilder.AppendLine("{");
            //codeBuilder.AppendLine($"    model = db.Query<{modelName}>(\"select * from {tableName}\").ToList();");
            //codeBuilder.AppendLine("}");



            codeBuilder.AppendLine("Dictionary<string,string> param = new Dictionary<string, string>();");
            foreach (FeplField field in fields.Where(fld => !string.IsNullOrEmpty(fld.foreignKeyId)))
            {
                codeBuilder.AppendLine($"param.Add(\"{field.name}\", {field.name});");
            }

            if (fields.Where(fld => !string.IsNullOrEmpty(fld.foreignKeyId)).Count() > 0)
            {
                codeBuilder.AppendLine($"\t\t\t\tList<{modelName}> model = {modelName}.Get(Utility.FillStyle.WithBasicNav, param);");
            }
            else
            {
                codeBuilder.AppendLine($"\t\t\t\tList<{modelName}> model = {modelName}.Get(Utility.FillStyle.AllProperties, param);");
            }


            codeBuilder.AppendLine("int v = 1;");
            codeBuilder.AppendLine($"DataTable dt = new DataTable(\"{tableName}\");");


            codeBuilder.AppendLine($"dt.Columns.AddRange(new DataColumn[{fields.Where(fld => fld.isHidden == 0).Count() + 1}]");  // static
            codeBuilder.AppendLine("{");

            codeBuilder.AppendLine($"    new DataColumn(\"S.No.\"),");
            foreach (var field in fields.Where(fld => fld.isHidden == 0))
            {
                codeBuilder.AppendLine($"    new DataColumn(\"{field.printName}\"),");
            }

            codeBuilder.AppendLine("});");
            codeBuilder.AppendLine($"foreach ({modelName} obj1 in model)");
            codeBuilder.AppendLine("{");
            codeBuilder.AppendLine("    dt.Rows.Add(");
            codeBuilder.AppendLine("        v.ToString()");  //static

            List<string> fieldLst = new List<string>();

            foreach (var field in fields.Where(fld => fld.isHidden == 0))
            {
                if (field.dataTypeNav.name == "Boolean")
                {
                    fieldLst.Add($"        (obj1.{field.name} ==1?\"Yes\":\"No\")");
                }
                else if (string.IsNullOrEmpty(field.foreignKeyId))
                {
                    fieldLst.Add($"        obj1.{field.name}");
                }
                else
                {
                    fieldLst.Add($"        obj1.{field.name.TrimId()}Nav?.name");
                }
            }
            if (fieldLst.Count > 0)
            {
                codeBuilder.AppendLine($", {string.Join(",", fieldLst)}  ");
            }
            codeBuilder.AppendLine("    );");
            codeBuilder.AppendLine("    v++;");
            codeBuilder.AppendLine("}");
            codeBuilder.AppendLine("using (XLWorkbook wb = new XLWorkbook())");
            codeBuilder.AppendLine("{");
            codeBuilder.AppendLine("    IXLWorksheet ws = wb.Worksheets.Add(dt);");
            codeBuilder.AppendLine("    using (MemoryStream stream = new MemoryStream())");
            codeBuilder.AppendLine("    {");
            codeBuilder.AppendLine("        wb.SaveAs(stream);");
            codeBuilder.AppendLine($"        return File(stream.ToArray(), \"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet\", \"{modelName}.xlsx\");");
            codeBuilder.AppendLine("    }");
            codeBuilder.AppendLine("}");
            codeBuilder.AppendLine("}");

            // PDF GENERATE

            codeBuilder.AppendLine($"public FileContentResult GeneratePdf(string id {insertColumns})");
            codeBuilder.AppendLine("{");

            //codeBuilder.AppendLine($"List<{modelName}> model = new List<{modelName}>();");
            //codeBuilder.AppendLine("using (IDbConnection db = new SQLiteConnection(Utility.ConnString))");
            //codeBuilder.AppendLine("{");
            //codeBuilder.AppendLine($"    model = db.Query<{modelName}>(\"select * from {tableName}\").ToList();");
            //codeBuilder.AppendLine("}");



            codeBuilder.AppendLine("Dictionary<string,string> param = new Dictionary<string, string>();");
            foreach (FeplField field in fields.Where(fld => !string.IsNullOrEmpty(fld.foreignKeyId)))
            {
                codeBuilder.AppendLine($"param.Add(\"{field.name}\", {field.name});");
            }

            if (fields.Where(fld => !string.IsNullOrEmpty(fld.foreignKeyId)).Count() > 0)
            {
                codeBuilder.AppendLine($"\t\t\t\tList<{modelName}> model = {modelName}.Get(Utility.FillStyle.WithBasicNav, param);");
            }
            else
            {
                codeBuilder.AppendLine($"\t\t\t\tList<{modelName}> model = {modelName}.Get(Utility.FillStyle.AllProperties, param);");
            }




            codeBuilder.AppendLine("iTextSharp.text.Font fonta = FontFactory.GetFont(\"Arial\", 8, iTextSharp.text.Font.UNDEFINED, BaseColor.BLACK);");
            codeBuilder.AppendLine("iTextSharp.text.Font fontb = FontFactory.GetFont(\"Arial\", 18, iTextSharp.text.Font.BOLD, BaseColor.BLACK);");
            codeBuilder.AppendLine("iTextSharp.text.Font fontc = FontFactory.GetFont(\"Arial\", 9, iTextSharp.text.Font.BOLD, BaseColor.BLACK);");
            codeBuilder.AppendLine("iTextSharp.text.Font fontd = FontFactory.GetFont(\"Arial\", 12, iTextSharp.text.Font.BOLD, BaseColor.BLACK);");
            codeBuilder.AppendLine("MemoryStream mmstream = new MemoryStream();");
            codeBuilder.AppendLine("iTextSharp.text.Document doc = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 15, 15, 15, 15);");
            codeBuilder.AppendLine("PdfWriter pdfWriter = PdfWriter.GetInstance(doc, mmstream);");
            codeBuilder.AppendLine("doc.Open();");
            codeBuilder.AppendLine("PdfContentByte cb = pdfWriter.DirectContent;");
            codeBuilder.AppendLine($"iTextSharp.text.Paragraph report = new iTextSharp.text.Paragraph(\"{tableName}\", fontb);");
            codeBuilder.AppendLine("report.Alignment = iTextSharp.text.Element.ALIGN_CENTER;");
            codeBuilder.AppendLine("report.Font = fontb;");
            codeBuilder.AppendLine("doc.Add(report);");




            foreach (FeplField field in fields.Where(fld => !string.IsNullOrEmpty(fld.foreignKeyId)))
            {

                codeBuilder.AppendLine($"if ({field.name} != null)");
                codeBuilder.AppendLine("{");
                codeBuilder.AppendLine("string a=\"\";");

                codeBuilder.AppendLine($" a={field.foreignKeyNav.tableNav.modelName}.GetById({field.name}).name;");
                codeBuilder.AppendLine($" string b=\"{field.foreignKeyNav.tableNav.modelName}\";");

                codeBuilder.AppendLine("iTextSharp.text.Paragraph report1 = new iTextSharp.text.Paragraph($\"{b}={a}\", fontd);");

                codeBuilder.AppendLine(" report.Alignment = iTextSharp.text.Element.ALIGN_RIGHT;");

                codeBuilder.AppendLine("report1.Font = fontd;");

                codeBuilder.AppendLine("doc.Add(report1);");

                codeBuilder.AppendLine("}");



            }



            codeBuilder.AppendLine($"PdfPTable table = new PdfPTable({fields.Where(fld => fld.isHidden == 0).Count() + 1});");       // static

            string a = "{";
            for (int i = 0; i <= fields.Where(fld => fld.isHidden == 0).Count(); i++)
            {
                if (i != fields.Where(fld => fld.isHidden == 0).Count())
                {
                    a += ".6f,";
                }
                else
                {
                    a += ".6f";
                }
            }
            a += "}";


            codeBuilder.AppendLine($"float[] widths = new float[] {a};");
            codeBuilder.AppendLine("table.SetWidths(widths);");
            codeBuilder.AppendLine("table.SpacingBefore = 20;");
            codeBuilder.AppendLine("table.TotalWidth = 560;");
            codeBuilder.AppendLine("table.LockedWidth = true;");
            codeBuilder.AppendLine("PdfPCell cell;");
            codeBuilder.AppendLine("cell = new PdfPCell(new Phrase(\"SR.No\", fontd));");     //static
            codeBuilder.AppendLine("cell.HorizontalAlignment = 1;");
            codeBuilder.AppendLine("table.AddCell(cell);");

            foreach (var field in fields.Where(fld => fld.isHidden == 0))
            {
                codeBuilder.AppendLine($"cell = new PdfPCell(new Phrase(\"{field.printName}\",fontd));");
                codeBuilder.AppendLine("cell.HorizontalAlignment = 1;");
                codeBuilder.AppendLine("table.AddCell(cell);");
            }


            codeBuilder.AppendLine("int v = 1;");
            codeBuilder.AppendLine($"foreach ({modelName} obj1 in model)");
            codeBuilder.AppendLine("{");

            codeBuilder.AppendLine("    cell = new PdfPCell(new Phrase(v.ToString(), fonta));");
            codeBuilder.AppendLine("    cell.HorizontalAlignment = 1;");
            codeBuilder.AppendLine("    table.AddCell(cell);");
            foreach (var field in fields.Where(fld => fld.isHidden == 0).ToList())
            {
                if (field.dataTypeNav.name == "Boolean")
                {
                    codeBuilder.AppendLine($"    cell = new PdfPCell(new Phrase((obj1.{field.name} ==1?\"Yes\":\"No\"), fonta));");
                }
                else if (field.dataTypeNav.name == "Integer" || field.dataTypeNav.name == "Decimal" || field.dataTypeNav.name == "Date")
                {
                    codeBuilder.AppendLine($"    cell = new PdfPCell(new Phrase(obj1.{field.name}.ToString(), fonta));");
                }
                else if (string.IsNullOrEmpty(field.foreignKeyId))
                {
                    codeBuilder.AppendLine($"    cell = new PdfPCell(new Phrase(obj1.{field.name}, fonta));");
                }
                else
                {
                    codeBuilder.AppendLine($"    cell = new PdfPCell(new Phrase(obj1.{field.name.TrimId()}Nav?.name, fonta));");
                }

                codeBuilder.AppendLine("    cell.HorizontalAlignment = 1;");
                codeBuilder.AppendLine("    table.AddCell(cell);");
            }

            codeBuilder.AppendLine("    v++;");
            codeBuilder.AppendLine("}");
            codeBuilder.AppendLine("doc.Add(table);");
            codeBuilder.AppendLine("pdfWriter.CloseStream = false;");
            codeBuilder.AppendLine("doc.Close();");
            codeBuilder.AppendLine("byte[] bytea = mmstream.ToArray();");
            codeBuilder.AppendLine($"return File(bytea, \"application/pdf\", \"{modelName}.pdf\");");


            // Close the controller class
            codeBuilder.AppendLine("   }");
            codeBuilder.AppendLine("  }");
            codeBuilder.AppendLine(" }");
            return codeBuilder.ToString();
        }
        public static string GenerateApiControllerCode(List<FeplField> fields, string modelName, string tableName, FeplProject project)
        {
            StringBuilder codeBuilder = new StringBuilder();

            // Generate controller class header
            codeBuilder.AppendLine("using System;");
            codeBuilder.AppendLine("using System.Collections.Generic;");
            codeBuilder.AppendLine("using Microsoft.AspNetCore.Authorization;");
            codeBuilder.AppendLine("using Microsoft.AspNetCore.Mvc;");
            codeBuilder.AppendLine("using System.Data.SQLite;");
            codeBuilder.AppendLine("using System.Data;");

            codeBuilder.AppendLine("using System.Linq;");
            codeBuilder.AppendLine("using Microsoft.AspNetCore.Cors;");
            codeBuilder.AppendLine("using Microsoft.AspNetCore.Hosting;");
            codeBuilder.AppendLine("using Microsoft.Extensions.Configuration;");
            codeBuilder.AppendLine("using Microsoft.Extensions.Logging;");

            codeBuilder.AppendLine("using Dapper;");
            codeBuilder.AppendLine($"using {project.name}.Models;");
            codeBuilder.AppendLine();
            codeBuilder.AppendLine($"namespace {project.name}.Controllers");
            codeBuilder.AppendLine("{");


            codeBuilder.AppendLine("  [Authorize]");
            codeBuilder.AppendLine("  [Route(\"api/[controller]\")]");
            codeBuilder.AppendLine("  [ApiController]");
            codeBuilder.AppendLine("  [EnableCors(\"AllowAllOrigins\")]");



            codeBuilder.AppendLine($"public class {modelName}ApiController : ControllerBase");

            codeBuilder.AppendLine("{");

            // constructor Code
            codeBuilder.AppendLine("\tprivate IConfiguration configuration;");
            codeBuilder.AppendLine("\tprivate readonly IWebHostEnvironment webHost;");
            codeBuilder.AppendLine($"\tprivate readonly ILogger<{modelName}ApiController> _logger;");
            codeBuilder.AppendLine();

            codeBuilder.AppendLine($"\tpublic {modelName}ApiController(IConfiguration _configuration, ILogger<{modelName}ApiController> logger, IWebHostEnvironment _webHost)");
            codeBuilder.AppendLine("\t{");
            codeBuilder.AppendLine("\t\tconfiguration = _configuration;");
            codeBuilder.AppendLine("\t\t_logger = logger;");
            codeBuilder.AppendLine("\t\twebHost = _webHost;");
            codeBuilder.AppendLine("\t}");

            //  Generate List method get
            codeBuilder.AppendLine("\t// List");
            codeBuilder.AppendLine("[AllowAnonymous]");
            codeBuilder.AppendLine("\t[HttpGet]");


            codeBuilder.AppendLine($"\tpublic IActionResult Get()");
            codeBuilder.AppendLine("\t{");

            codeBuilder.AppendLine("\t\ttry");
            codeBuilder.AppendLine("\t\t{");

            codeBuilder.AppendLine($"\t\t\t\tList<{modelName}> records = {modelName}.Get(Utility.FillStyle.AllProperties);");



            codeBuilder.AppendLine("\t\t\t\treturn Ok(records);");
            codeBuilder.AppendLine("\t\t\t}");
            codeBuilder.AppendLine("\t\tcatch (Exception ex)");
            codeBuilder.AppendLine("\t\t{");
            codeBuilder.AppendLine("\t\t\t// Log the exception or handle it as needed");
            codeBuilder.AppendLine("\t\t\treturn StatusCode(500, \"An error occurred while retrieving data from the database.\");");
            codeBuilder.AppendLine("\t\t}");
            codeBuilder.AppendLine("\t}");


            //Get specific


            codeBuilder.AppendLine("\t// List");
            codeBuilder.AppendLine("\t[HttpGet(\"{id}\")]");


            codeBuilder.AppendLine($"\tpublic IActionResult Get(string id)");
            codeBuilder.AppendLine("\t{");

            codeBuilder.AppendLine("\t\ttry");
            codeBuilder.AppendLine("\t\t{");

            codeBuilder.AppendLine($"\t\t\t\t{modelName} records = {modelName}.GetById(id,Utility.FillStyle.AllProperties);");



            codeBuilder.AppendLine("\t\t\t\treturn Ok(records);");
            codeBuilder.AppendLine("\t\t\t}");
            codeBuilder.AppendLine("\t\tcatch (Exception ex)");
            codeBuilder.AppendLine("\t\t{");
            codeBuilder.AppendLine("\t\t\t// Log the exception or handle it as needed");
            codeBuilder.AppendLine("\t\t\treturn StatusCode(500, \"An error occurred while retrieving data from the database.\");");
            codeBuilder.AppendLine("\t\t}");
            codeBuilder.AppendLine("\t}");




            // Generate Create (POST) method
            codeBuilder.AppendLine("\t// POST: Create a new record");
            codeBuilder.AppendLine("\t[HttpPost]");

            codeBuilder.AppendLine($"\tpublic IActionResult Post({modelName} model)");

            codeBuilder.AppendLine("\t{");

            codeBuilder.AppendLine("\t\tusing (IDbConnection db = new SQLiteConnection(Utility.ConnString))");
            codeBuilder.AppendLine("\t\t{");
            codeBuilder.AppendLine("\t\t\tdb.Open();");
            codeBuilder.AppendLine("\t\t\tusing (var transaction = db.BeginTransaction())");
            codeBuilder.AppendLine("\t\t\t{");
            codeBuilder.AppendLine("\t\t\t\ttry");
            codeBuilder.AppendLine("\t\t\t\t{");
            if (fields.Where(fld => !string.IsNullOrEmpty(fld.foreignKeyId)).Count() > 0)
            {
                foreach (FeplField field in fields.Where(fld => !string.IsNullOrEmpty(fld.foreignKeyId)))
                {
                    codeBuilder.AppendLine($"if (string.IsNullOrEmpty(model.{field.foreignKeyNav.tableNav.modelName}))");
                    codeBuilder.AppendLine("\t{");
                    codeBuilder.AppendLine($"model.{field.foreignKeyNav.tableNav.modelName} = null;");
                    codeBuilder.AppendLine("\t}");
                }
            }

            codeBuilder.AppendLine("\t\t\t\t\tmodel.idPrefix = \"F\";");
            codeBuilder.AppendLine($"\t\t\t\t\tmodel.numId = db.ExecuteScalar<int>(\"{Utility.MaxIdQuery(fields.FirstOrDefault()?.tableNav?.id ?? "")}\") + 1;");
            codeBuilder.AppendLine("\t\t\t\t\tmodel.id = model.idPrefix + model.numId;");
            codeBuilder.AppendLine($"\t\t\t\t\tstring sql = \"{Utility.InsertQuery(fields.FirstOrDefault()?.tableNav?.id ?? "")}\";");
            codeBuilder.AppendLine($"\t\t\t\t\tint affectedRows = db.Execute(sql, model, transaction);");
            List<FeplField> fieldm = FeplField.Get(Utility.FillStyle.WithFullNav);
            if (fields.Where(o => o.metaRequired == 1).Count() > 0)
            {
                string fieldname = fields.Where(o => o.metaRequired == 1).FirstOrDefault().printName.ToLower();
                string tablename = fields.Where(o => o.metaRequired == 1).FirstOrDefault().printName;

                codeBuilder.AppendLine($"\t\tforeach ({tablename} {tablename.ToLower()} in model.{fieldname})");
                codeBuilder.AppendLine("\t\t {");

                codeBuilder.AppendLine($"\t\t{tablename.ToLower()}.numId = db.ExecuteScalar<int>(\"select max(numId) from {fieldm.Where(o => o.tableId == fields.Where(o => o.metaRequired == 1).FirstOrDefault().metaTableId).FirstOrDefault()?.tableNav.name}\",\"\",transaction) + 1;");
                codeBuilder.AppendLine($"\t\t{tablename.ToLower()}.idPrefix = \"F\";");
                codeBuilder.AppendLine($"\t\t{tablename.ToLower()}.id = {tablename.ToLower()}.idPrefix+{tablename.ToLower()}.numId;");
                codeBuilder.AppendLine($"\t\t{tablename.ToLower()}.{modelName.ToLower()}Id = model.id;");
                codeBuilder.AppendLine($"\t\tdb.Execute(\"{Utility.InsertQuery(fields.Where(a => a.metaRequired == 1).FirstOrDefault().metaTableId ?? "")}\",{tablename.ToLower()}, transaction);");
                codeBuilder.AppendLine("\t\t}");
            }
            codeBuilder.AppendLine("\t\t\t\t\ttransaction.Commit();");
            codeBuilder.AppendLine("\t\t\treturn Ok(new{Message=\"Success\"});");
            codeBuilder.AppendLine("\t\t\t\t}");
            codeBuilder.AppendLine("\t\t\t\tcatch (Exception ex)");
            codeBuilder.AppendLine("\t\t\t\t{");
            codeBuilder.AppendLine("\t\t\t\t\ttransaction.Rollback();");
            codeBuilder.AppendLine("\t\t\t\t\treturn BadRequest(new {ex.Message});");
            codeBuilder.AppendLine("\t\t\t\t}");
            codeBuilder.AppendLine("\t\t\t}");
            codeBuilder.AppendLine("\t\t}");
            codeBuilder.AppendLine("\t}");


            // Generate Edit (POST) method

            codeBuilder.AppendLine("\t// POST: Create a new record");
            codeBuilder.AppendLine("\t[HttpPut]");
            codeBuilder.AppendLine($"\tpublic IActionResult Put({modelName} model)");
            codeBuilder.AppendLine("\t{");

            codeBuilder.AppendLine("\t\tusing (IDbConnection db = new SQLiteConnection(Utility.ConnString))");
            codeBuilder.AppendLine("\t\t{");
            codeBuilder.AppendLine("\t\t\tdb.Open();");
            codeBuilder.AppendLine("\t\t\tusing (var transaction = db.BeginTransaction())");
            codeBuilder.AppendLine("\t\t\t{");
            codeBuilder.AppendLine("\t\t\t\ttry");
            codeBuilder.AppendLine("\t\t\t\t{");

            string vardecl = "";
            foreach (FeplField field in fields)
            {
                if (field.name == "id" || field.name == "numId" || field.name == "idPrefix")
                {

                }
                else
                {
                    vardecl += "" + field.name + "=@" + field.name + " ,";
                }
            }
            vardecl = vardecl.Remove(vardecl.Length - 1);
            if (fields.Where(fld => !string.IsNullOrEmpty(fld.foreignKeyId)).Count() > 0)
            {
                foreach (FeplField field in fields.Where(fld => !string.IsNullOrEmpty(fld.foreignKeyId)))
                {
                    codeBuilder.AppendLine($"if (string.IsNullOrEmpty(model.{field.foreignKeyNav.tableNav.modelName}))");
                    codeBuilder.AppendLine("\t{");
                    codeBuilder.AppendLine($"model.{field.foreignKeyNav.tableNav.modelName} = null;");
                    codeBuilder.AppendLine("\t}");
                }
            }
            codeBuilder.AppendLine($"\t\t\t\t\tstring sql = \"update {tableName} set {vardecl} where id = @id\";");

            codeBuilder.AppendLine($"\t\t\t\t\tint affectedRows = db.Execute(sql, model, transaction);");

            if (fields.Where(o => o.metaRequired == 1).Count() > 0)
            {
                string fieldname = fields.Where(o => o.metaRequired == 1).FirstOrDefault().printName.ToLower();
                string tablename = fields.Where(o => o.metaRequired == 1).FirstOrDefault().printName;
                codeBuilder.AppendLine($" db.Execute(\"DELETE FROM {fieldm.Where(o => o.tableId == fields.Where(o => o.metaRequired == 1).FirstOrDefault()?.metaTableId).FirstOrDefault()?.tableNav.name} WHERE {modelName.ToLower()}Id = '\"+ model.id+\"'\",\"\",transaction);");
                codeBuilder.AppendLine($"\t\tforeach ({tablename} {tablename.ToLower()} in model.{fieldname})");
                codeBuilder.AppendLine("\t\t {");
                codeBuilder.AppendLine($"\t\t{tablename.ToLower()}.numId = db.ExecuteScalar<int>(\"select max(numId) from {fieldm.Where(o => o.tableId == fields.Where(o => o.metaRequired == 1).FirstOrDefault()?.metaTableId).FirstOrDefault()?.tableNav.name}\",\"\",transaction) + 1;");
                codeBuilder.AppendLine($"\t\t{tablename.ToLower()}.idPrefix = \"F\";");
                codeBuilder.AppendLine($"\t\t{tablename.ToLower()}.id = {tablename.ToLower()}.numId+{tablename.ToLower()}.idPrefix;");
                codeBuilder.AppendLine($"\t\t{tablename.ToLower()}.{modelName.ToLower()}Id = model.id;");

                codeBuilder.AppendLine($"\t\tdb.Execute(\"{Utility.InsertQuery(fields.Where(a => a.metaRequired == 1).FirstOrDefault()?.metaTableId ?? "")}\",{tablename.ToLower()},transaction);");
                codeBuilder.AppendLine("\t\t}");
            }

            codeBuilder.AppendLine("\t\t\t\t\ttransaction.Commit();");
            codeBuilder.AppendLine("\t\t\treturn Ok(new{Message=\"Updated\"});");
            codeBuilder.AppendLine("\t\t\t\t}");
            codeBuilder.AppendLine("\t\t\t\tcatch (Exception ex)");
            codeBuilder.AppendLine("\t\t\t\t{");
            codeBuilder.AppendLine("\t\t\t\t\ttransaction.Rollback();");
            codeBuilder.AppendLine("\t\t\t\t\treturn BadRequest(new {ex.Message});");
            codeBuilder.AppendLine("\t\t\t\t}");
            codeBuilder.AppendLine("\t\t\t}");
            codeBuilder.AppendLine("\t\t}");
            codeBuilder.AppendLine("\t}");


            // Generate Delete (POST) method


            codeBuilder.AppendLine("\t// POST: Create a new record");
            codeBuilder.AppendLine("\t[HttpDelete]");
            //codeBuilder.AppendLine($"\tpublic IActionResult Delete({modelName} model)");
            codeBuilder.AppendLine($"\tpublic IActionResult Delete(string id)");
            codeBuilder.AppendLine("\t{");

            //codeBuilder.AppendLine("\t\t if (model.Delete())");
            //codeBuilder.AppendLine("\t\t{");

            codeBuilder.AppendLine("\t\tusing (IDbConnection db = new SQLiteConnection(Utility.ConnString))");
            codeBuilder.AppendLine("\t\t{");
            codeBuilder.AppendLine("\t\t\tdb.Open();");
            codeBuilder.AppendLine("\t\t\tusing (var transaction = db.BeginTransaction())");
            codeBuilder.AppendLine("\t\t\t{");
            codeBuilder.AppendLine("\t\t\t\ttry");
            codeBuilder.AppendLine("\t\t\t\t{");
            codeBuilder.AppendLine($"\t\t\t\t\tstring sql = \"{Utility.DeleteQuery(fields.FirstOrDefault()?.tableNav?.id ?? "")}\";");
            //codeBuilder.AppendLine("\t\t\t\t\tint affectedRows = db.Execute(sql, new { id = model.id }, transaction);");
            codeBuilder.AppendLine("\t\t\t\t\tint affectedRows = db.Execute(sql, new { id = id }, transaction);");
            if (fields.Where(o => o.metaRequired == 1).Count() > 0)
            {
                string fieldname = fields.Where(o => o.metaRequired == 1).FirstOrDefault().printName.ToLower();
                string tablename = fields.Where(o => o.metaRequired == 1).FirstOrDefault().printName;
                codeBuilder.AppendLine($" db.Execute(\"DELETE FROM {fieldm.Where(o => o.tableId == fields.Where(o => o.metaRequired == 1).FirstOrDefault()?.metaTableId).FirstOrDefault()?.tableNav.name} WHERE {modelName.ToLower()}Id = '\"+ id+\"'\",\"\",transaction);");
            }
            codeBuilder.AppendLine("\t\t\t\t\ttransaction.Commit();");
            codeBuilder.AppendLine("\t\t\treturn Ok(new{Message=\"Deleted\"});");
            codeBuilder.AppendLine("\t\t}");
            codeBuilder.AppendLine("\t\tcatch(Exception ex)");
            codeBuilder.AppendLine("\t\t{");
            codeBuilder.AppendLine("\t\t\treturn BadRequest(new {ex.Message});");
            codeBuilder.AppendLine("\t\t}");
            codeBuilder.AppendLine("\t}");
            codeBuilder.AppendLine("\t}");
            codeBuilder.AppendLine("\t}");









            // Close the controller class

            codeBuilder.AppendLine("  }");
            codeBuilder.AppendLine(" }");
            return codeBuilder.ToString();
        }
        public static string GenerateLoginController(string projectName)
        {
            StringBuilder codeBuilder = new StringBuilder();

            codeBuilder.AppendLine("using Microsoft.AspNetCore.Mvc;");
            codeBuilder.AppendLine("using Microsoft.AspNetCore.Authentication;");
            codeBuilder.AppendLine("using Microsoft.AspNetCore.Authentication.Cookies;");
            codeBuilder.AppendLine("using Microsoft.AspNetCore.Authorization;");
            codeBuilder.AppendLine("using Microsoft.IdentityModel.Tokens;");
            codeBuilder.AppendLine("using System.IdentityModel.Tokens.Jwt;");
            codeBuilder.AppendLine("using System.Text;");
            codeBuilder.AppendLine("using Newtonsoft.Json;");
            codeBuilder.AppendLine($"using {projectName}.Models;");
            codeBuilder.AppendLine("using Dapper;");
            codeBuilder.AppendLine("using System.Data;");
            codeBuilder.AppendLine("using System.Data.SQLite;");
            codeBuilder.AppendLine("using System.Security.Claims;");
            codeBuilder.AppendLine();
            codeBuilder.AppendLine($"namespace {projectName}.Controllers");
            codeBuilder.AppendLine("{");
            //codeBuilder.AppendLine("\t[AllowAnonymous]");
            codeBuilder.AppendLine("\tpublic class LoginController : Controller");
            codeBuilder.AppendLine("\t{");
            codeBuilder.AppendLine("\t\tprivate IConfiguration configuration;");
            codeBuilder.AppendLine("\t\tprivate readonly IWebHostEnvironment webHost;");
            codeBuilder.AppendLine("\t\tprivate readonly ILogger<LoginController> _logger;");
            codeBuilder.AppendLine();
            codeBuilder.AppendLine("\t\tpublic LoginController(IConfiguration _configuration, ILogger<LoginController> logger, IWebHostEnvironment _webHost)");
            codeBuilder.AppendLine("\t\t{");
            codeBuilder.AppendLine("\t\t\tconfiguration = _configuration;");
            codeBuilder.AppendLine("\t\t\t_logger = logger;");
            codeBuilder.AppendLine("\t\t\twebHost = _webHost;");
            codeBuilder.AppendLine("\t\t}");
            codeBuilder.AppendLine();
            codeBuilder.AppendLine("\t\tpublic IActionResult Index()");
            codeBuilder.AppendLine("\t\t{");
            codeBuilder.AppendLine("\t\t\treturn View();");
            codeBuilder.AppendLine("\t\t}");
            codeBuilder.AppendLine();
            codeBuilder.AppendLine("\t\t[AllowAnonymous]");
            codeBuilder.AppendLine("\t\t[HttpPost]");
            codeBuilder.AppendLine("\t\tpublic IActionResult Index(LoginUser user)");
            codeBuilder.AppendLine("\t\t{");
            codeBuilder.AppendLine("\t\t\tusing (IDbConnection db = new SQLiteConnection(Utility.ConnString))");
            codeBuilder.AppendLine("\t\t\t{");
            //codeBuilder.AppendLine("\t\t\tIActionResult response = Unauthorized();");
            //codeBuilder.AppendLine("\t\t\tvar _user = AuthenticateUser(user);");
            //codeBuilder.AppendLine("\t\t\tif (_user != null)");
            //codeBuilder.AppendLine("\t\t\t{");
            //codeBuilder.AppendLine("\t\t\tvar token = GenerateToken(_user);");
            //codeBuilder.AppendLine("\t\t\tresponse = Ok(new { token = token });");
            //codeBuilder.AppendLine("\t\t\t}");
            //codeBuilder.AppendLine("\t\t\treturn response;");
            codeBuilder.AppendLine("\t\t\t\tuser = db.Query<LoginUser>(\"select * from LoginUsers where name=@name and password = @password\", user).FirstOrDefault();");
            codeBuilder.AppendLine();
            codeBuilder.AppendLine("\t\t\t\tif (user != null)");
            codeBuilder.AppendLine("\t\t\t\t{");
            //codeBuilder.AppendLine("\t\t\t\t\tLogin(user);");
            codeBuilder.AppendLine("//if using token validation this section this may cause issue");
            codeBuilder.AppendLine("//ClaimsIdentity identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);");
            codeBuilder.AppendLine("//identity.AddClaim(new Claim(ClaimTypes.Name, user.name));");
            codeBuilder.AppendLine();
            codeBuilder.AppendLine("//HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));");
            codeBuilder.AppendLine("//working for this in program.cs is commented");
            codeBuilder.AppendLine();
            codeBuilder.AppendLine();
            codeBuilder.AppendLine("\t\t\t\t\treturn RedirectToAction(\"Index\", \"Home\");");
            codeBuilder.AppendLine("\t\t\t\t}");
            codeBuilder.AppendLine("\t\t\t\telse");
            codeBuilder.AppendLine("\t\t\t\t{");
            codeBuilder.AppendLine("\t\t\t\t\tViewBag.u = \"Invalid Username and Password\";");
            codeBuilder.AppendLine("\t\t\t\t\treturn View();");
            codeBuilder.AppendLine("\t\t\t\t}");
            codeBuilder.AppendLine("\t\t\t}");
            codeBuilder.AppendLine("\t\t\t}");
            codeBuilder.AppendLine("\t\tprivate LoginUser AuthenticateUser(LoginUser user)");
            codeBuilder.AppendLine("\t\t{");
            codeBuilder.AppendLine("\t\tif (user != null)");
            codeBuilder.AppendLine("\t\t{");
            codeBuilder.AppendLine("\t\tusing (IDbConnection db = new SQLiteConnection(Utility.ConnString))");
            codeBuilder.AppendLine("\t\t{");
            codeBuilder.AppendLine("\t\tLoginUser loginUser = db.Query<LoginUser>(\"select * from LoginUsers where name = @name and password = @password\", user).FirstOrDefault();");
            codeBuilder.AppendLine("\t\tif (user.name == loginUser.name && user.password == loginUser.password)");
            codeBuilder.AppendLine("\t\t{");
            codeBuilder.AppendLine("\t\tloginUser = new LoginUser { name = user.name, password = user.password };");
            codeBuilder.AppendLine("\t\t}");
            codeBuilder.AppendLine("\t\treturn loginUser;");
            codeBuilder.AppendLine("\t\t}");
            codeBuilder.AppendLine("\t\t}");
            codeBuilder.AppendLine("\t\telse");
            codeBuilder.AppendLine("\t\t{");
            codeBuilder.AppendLine("\t\treturn user;");
            codeBuilder.AppendLine("\t\t}");
            codeBuilder.AppendLine("\t\t}");
            codeBuilder.AppendLine();
            codeBuilder.AppendLine("\t\tprivate string GenerateToken(LoginUser user)");
            codeBuilder.AppendLine("\t\t{");
            codeBuilder.AppendLine("\t\tvar securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration[\"Jwt:key\"]));");
            codeBuilder.AppendLine("\t\tvar credentials = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);");
            codeBuilder.AppendLine("\t\tvar tokenn = new JwtSecurityToken(configuration[\"Jwt:Issuer\"], configuration[\"Jwt:Audience\"], null,");
            codeBuilder.AppendLine("\t\t    expires: DateTime.Now.AddHours(4), signingCredentials: credentials);");
            codeBuilder.AppendLine("//Token will valid for 4 Hours you can change it according to your project requirements minutes,days,hours etc.");
            codeBuilder.AppendLine("//For Configuration go to appsetting.json");
            codeBuilder.AppendLine("\t\treturn new JwtSecurityTokenHandler().WriteToken(tokenn);");
            codeBuilder.AppendLine("\t\t}");
            codeBuilder.AppendLine();
            codeBuilder.AppendLine("\t\t[AllowAnonymous]");
            codeBuilder.AppendLine("\t\t[HttpPost]");
            codeBuilder.AppendLine("\t\tpublic IActionResult Login(LoginUser user)");
            codeBuilder.AppendLine("\t\t{");
            codeBuilder.AppendLine("\t\t    using (IDbConnection db = new SQLiteConnection(Utility.ConnString))");
            codeBuilder.AppendLine("\t\t    {");
            codeBuilder.AppendLine("\t\t        user = db.Query<LoginUser>(\"select * from LoginUsers where name=@name and password = @password\", user).FirstOrDefault();");

            codeBuilder.AppendLine("\t\t        IActionResult response = Unauthorized();");
            codeBuilder.AppendLine("\t\t        var _user = AuthenticateUser(user);");
            codeBuilder.AppendLine("\t\t        if (_user != null)");
            codeBuilder.AppendLine("\t\t        {");
            codeBuilder.AppendLine("\t\t            var token = GenerateToken(_user);");
            codeBuilder.AppendLine("\t\t            response = Ok(new { token = token });");
            codeBuilder.AppendLine("\t\t             if (response is OkObjectResult okObjectResult)");
            codeBuilder.AppendLine("\t\t             {");
            codeBuilder.AppendLine("\t\t             string jsonResponse = JsonConvert.SerializeObject(okObjectResult.Value);");
            codeBuilder.AppendLine("\t\t             var jsonObject = JsonConvert.DeserializeObject<dynamic>(jsonResponse);");
            codeBuilder.AppendLine("\t\t             Utility.response = jsonObject.token;");
            codeBuilder.AppendLine("\t\t             }");
            codeBuilder.AppendLine("\t\t        }");
            codeBuilder.AppendLine("\t\t        else");
            codeBuilder.AppendLine("\t\t        {");
            codeBuilder.AppendLine("\t\t        response = BadRequest(new { Message = \"Invalid Username or Password\" });");
            codeBuilder.AppendLine("\t\t        }");
            codeBuilder.AppendLine("\t\t        return response;");
            codeBuilder.AppendLine("\t\t    }");
            codeBuilder.AppendLine("\t\t}");

            codeBuilder.AppendLine();
            codeBuilder.AppendLine("\t\tpublic bool IsTokenExpired(string token)");
            codeBuilder.AppendLine("\t\t{");
            codeBuilder.AppendLine("\t\t    var tokenHandler = new JwtSecurityTokenHandler();");
            codeBuilder.AppendLine("\t\t    var key = Encoding.UTF8.GetBytes(configuration[\"Jwt:Key\"]);");
            codeBuilder.AppendLine("\t\t    var validationParameters = new TokenValidationParameters");
            codeBuilder.AppendLine("\t\t    {");
            codeBuilder.AppendLine("\t\t        ValidateIssuer = true,");
            codeBuilder.AppendLine("\t\t        ValidateAudience = true,");
            codeBuilder.AppendLine("\t\t        ValidateLifetime = true,");
            codeBuilder.AppendLine("\t\t        ValidIssuer = configuration[\"Jwt:Issuer\"],");
            codeBuilder.AppendLine("\t\t        ValidAudience = configuration[\"Jwt:Audience\"],");
            codeBuilder.AppendLine("\t\t        IssuerSigningKey = new SymmetricSecurityKey(key),");
            codeBuilder.AppendLine("\t\t        ClockSkew = TimeSpan.Zero // Optional: to handle clock skew");
            codeBuilder.AppendLine("\t\t    };");
            codeBuilder.AppendLine("\t\t    try");
            codeBuilder.AppendLine("\t\t    {");
            codeBuilder.AppendLine("\t\t        tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);");
            codeBuilder.AppendLine("\t\t        return false; // Token is valid and not expired");
            codeBuilder.AppendLine("\t\t    }");
            codeBuilder.AppendLine("\t\t    catch (SecurityTokenExpiredException)");
            codeBuilder.AppendLine("\t\t    {");
            codeBuilder.AppendLine("\t\t        return true; // Token is expired");
            codeBuilder.AppendLine("\t\t    }");
            codeBuilder.AppendLine("\t\t    catch (Exception)");
            codeBuilder.AppendLine("\t\t    {");
            codeBuilder.AppendLine("\t\t        return true; // Token is invalid");
            codeBuilder.AppendLine("\t\t    }");
            codeBuilder.AppendLine("\t\t}");
            codeBuilder.AppendLine("\t\t[AllowAnonymous]");
            codeBuilder.AppendLine("\t\t[HttpPost]");
            codeBuilder.AppendLine("\t\tpublic IActionResult Token(string token)");
            codeBuilder.AppendLine("\t\t{");
            codeBuilder.AppendLine("\t\t    using (IDbConnection db = new SQLiteConnection(Utility.ConnString))");
            codeBuilder.AppendLine("\t\t    {");
            codeBuilder.AppendLine("\t\t        bool isExpired = IsTokenExpired(token);");
            codeBuilder.AppendLine("\t\t        if (isExpired)");
            codeBuilder.AppendLine("\t\t        {");
            codeBuilder.AppendLine("\t\t            return Unauthorized(\"Token is expired.\");");
            codeBuilder.AppendLine("\t\t        }");
            codeBuilder.AppendLine("\t\t        return Ok(\"Token is valid.\");");
            codeBuilder.AppendLine("\t\t    }");
            codeBuilder.AppendLine("\t\t}");
            codeBuilder.AppendLine();

            codeBuilder.AppendLine("\t\tpublic IActionResult Logout()");
            codeBuilder.AppendLine("\t\t{");
            codeBuilder.AppendLine("\t\t\tHttpContext.SignOutAsync();");
            codeBuilder.AppendLine("\t\t\treturn RedirectToAction(\"Index\");");
            codeBuilder.AppendLine("\t\t}");
            codeBuilder.AppendLine("\t}");
            codeBuilder.AppendLine("}");

            return codeBuilder.ToString();
        }
        public static string GenerateLoginIndexView()
        {
            StringBuilder codeBuilder = new StringBuilder();

            codeBuilder.AppendLine("@{");
            codeBuilder.AppendLine("    Layout = null;");
            codeBuilder.AppendLine("}");

            codeBuilder.AppendLine("<!DOCTYPE html>");
            codeBuilder.AppendLine("<html lang=\"en\">");
            codeBuilder.AppendLine("<head>");
            codeBuilder.AppendLine("    <meta charset=\"UTF-8\">");
            codeBuilder.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            codeBuilder.AppendLine("    <style>");
            codeBuilder.AppendLine("        *{");
            codeBuilder.AppendLine("            margin: 0;");
            codeBuilder.AppendLine("            padding: 0;");
            codeBuilder.AppendLine("            box-sizing: border-box;");
            codeBuilder.AppendLine("        }");
            codeBuilder.AppendLine("        body{");
            codeBuilder.AppendLine("            min-height: 100vh;");
            codeBuilder.AppendLine("            background: #eee;");
            codeBuilder.AppendLine("            display: flex;");
            codeBuilder.AppendLine("            font-family: sans-serif;");
            codeBuilder.AppendLine("        }");
            codeBuilder.AppendLine("        .container{");
            codeBuilder.AppendLine("            margin: auto;");
            codeBuilder.AppendLine("            width: 500px;");
            codeBuilder.AppendLine("            max-width: 90%;");
            codeBuilder.AppendLine("        }");
            codeBuilder.AppendLine("        .container form{");
            codeBuilder.AppendLine("            width: 100%;");
            codeBuilder.AppendLine("            height: 100%;");
            codeBuilder.AppendLine("            padding: 20px;");
            codeBuilder.AppendLine("            background: white;");
            codeBuilder.AppendLine("            border-radius: 4px;");
            codeBuilder.AppendLine("            box-shadow: 0 8px 16px rgba(0, 0, 0, .3);");
            codeBuilder.AppendLine("        }");
            codeBuilder.AppendLine("        .container form h1{");
            codeBuilder.AppendLine("            text-align: center;");
            codeBuilder.AppendLine("            margin-bottom: 24px;");
            codeBuilder.AppendLine("            color: #222;");
            codeBuilder.AppendLine("        }");
            codeBuilder.AppendLine("        .container form .form-control{");
            codeBuilder.AppendLine("            width: 100%;");
            codeBuilder.AppendLine("            height: 40px;");
            codeBuilder.AppendLine("            background: white;");
            codeBuilder.AppendLine("            border-radius: 4px;");
            codeBuilder.AppendLine("            border: 1px solid silver;");
            codeBuilder.AppendLine("            margin: 10px 0 18px 0;");
            codeBuilder.AppendLine("            padding: 0 10px;");
            codeBuilder.AppendLine("        }");
            codeBuilder.AppendLine("        .container form .btn{");
            codeBuilder.AppendLine("            margin-left: 50%;");
            codeBuilder.AppendLine("            transform: translateX(-50%);");
            codeBuilder.AppendLine("            width: 120px;");
            codeBuilder.AppendLine("            height: 34px;");
            codeBuilder.AppendLine("            border: none;");
            codeBuilder.AppendLine("            outline: none;");
            codeBuilder.AppendLine("            background: #27a327;");
            codeBuilder.AppendLine("            cursor: pointer;");
            codeBuilder.AppendLine("            font-size: 16px;");
            codeBuilder.AppendLine("            text-transform: uppercase;");
            codeBuilder.AppendLine("            color: white;");
            codeBuilder.AppendLine("            border-radius: 4px;");
            codeBuilder.AppendLine("            transition: .3s;");
            codeBuilder.AppendLine("        }");
            codeBuilder.AppendLine("        .container form .btn:hover{");
            codeBuilder.AppendLine("            opacity: .7;");
            codeBuilder.AppendLine("        }");
            codeBuilder.AppendLine("        .alert{");
            codeBuilder.AppendLine("        padding:10px 20px 10px 20px;");
            codeBuilder.AppendLine("        background-color:red;");
            codeBuilder.AppendLine("        color:#fff;");
            codeBuilder.AppendLine("        text-align:center;");
            codeBuilder.AppendLine("        }");
            codeBuilder.AppendLine("    </style>");
            codeBuilder.AppendLine("    <title>Login Form</title>");
            codeBuilder.AppendLine("</head>");
            codeBuilder.AppendLine("<body>");
            codeBuilder.AppendLine("<div class=\"container\">");
            codeBuilder.AppendLine("@if (ViewBag.u != null)");
            codeBuilder.AppendLine("{");
            codeBuilder.AppendLine("    <div class=\"alert\" role=\"alert\">");
            codeBuilder.AppendLine("    @ViewBag.u");
            codeBuilder.AppendLine("     </div>");
            codeBuilder.AppendLine("}");
            codeBuilder.AppendLine("    <form method=\"post\">");
            codeBuilder.AppendLine("        <h2>Login Form</h2>");
            codeBuilder.AppendLine("        <div class=\"form-group\">");
            codeBuilder.AppendLine("            <label for=\"username\">Username</label>");
            codeBuilder.AppendLine("            <input type=\"text\" name=\"name\" class=\"form-control\" placeholder=\"Enter Username\">");
            codeBuilder.AppendLine("        </div>");
            codeBuilder.AppendLine("        <div class=\"form-group\">");
            codeBuilder.AppendLine("            <label for=\"password\">Password</label>");
            codeBuilder.AppendLine("            <input type=\"password\" name=\"password\" class=\"form-control\" placeholder=\"Enter password\">");
            codeBuilder.AppendLine("        </div>");
            codeBuilder.AppendLine("        <input type=\"submit\" class=\"btn\" value=\"Login\">");
            codeBuilder.AppendLine("    </form>");
            codeBuilder.AppendLine("</div>");
            codeBuilder.AppendLine("</body>");
            codeBuilder.AppendLine("</html>");

            return codeBuilder.ToString();
        }
        public static string GenerateProgramCS(int isLoginRequired, int Api, int mvcApi)
        {
            StringBuilder codeBuilder = new StringBuilder();
            if (isLoginRequired == 1 && Api == 0)
            {
                codeBuilder.AppendLine("//using Microsoft.AspNetCore.Authentication.Cookies;");
            }
            codeBuilder.AppendLine("using Microsoft.AspNetCore.Authentication.JwtBearer;");
            codeBuilder.AppendLine("using Microsoft.IdentityModel.Tokens;");
            codeBuilder.AppendLine("using System.Text;");
            codeBuilder.AppendLine();
            codeBuilder.AppendLine("var builder = WebApplication.CreateBuilder(args);");
            codeBuilder.AppendLine("builder.Configuration.AddJsonFile(\"appsettings.json\");");
            codeBuilder.AppendLine();
            codeBuilder.AppendLine("// Add services to the container.");
            codeBuilder.AppendLine("builder.Services.AddControllersWithViews();");
            codeBuilder.AppendLine("builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)");
            codeBuilder.AppendLine(".AddJwtBearer(options =>");
            codeBuilder.AppendLine("{");
            codeBuilder.AppendLine("options.TokenValidationParameters = new TokenValidationParameters");
            codeBuilder.AppendLine("{");
            codeBuilder.AppendLine(" ValidateIssuer = true,");
            codeBuilder.AppendLine(" ValidateAudience = true,");
            codeBuilder.AppendLine(" ValidateLifetime = true,");
            codeBuilder.AppendLine(" ValidIssuer = builder.Configuration[\"Jwt:Issuer\"],");
            codeBuilder.AppendLine(" ValidAudience = builder.Configuration[\"Jwt:Audience\"],");
            codeBuilder.AppendLine("IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration[\"Jwt:Key\"]))");
            codeBuilder.AppendLine("};");
            codeBuilder.AppendLine(" });");
            codeBuilder.AppendLine();
            if (isLoginRequired == 1 && Api == 0)
            {
                codeBuilder.AppendLine("//Both Token and CookieAuthentication can't work at same time may cause issue");
                codeBuilder.AppendLine("//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)");
                codeBuilder.AppendLine("//.AddCookie(abc =>");
                codeBuilder.AppendLine("//{");
                codeBuilder.AppendLine("//    abc.LoginPath = Microsoft.AspNetCore.Http.PathString.FromUriComponent(\"/Login/Index\");");
                codeBuilder.AppendLine("//});");
            }
            if (Api == 1 || mvcApi == 1)
            {
                codeBuilder.AppendLine("builder.Services.AddCors(options =>");
                codeBuilder.AppendLine("{");
                codeBuilder.AppendLine("options.AddPolicy(\"AllowAllOrigins\",");
                codeBuilder.AppendLine("builder =>");
                codeBuilder.AppendLine("{");
                codeBuilder.AppendLine("builder.AllowAnyOrigin()");

                codeBuilder.AppendLine(".AllowAnyHeader()");
                codeBuilder.AppendLine(".AllowAnyMethod();");
                codeBuilder.AppendLine("});");
                codeBuilder.AppendLine("});");

            }


            codeBuilder.AppendLine();
            codeBuilder.AppendLine("var app = builder.Build();");
            codeBuilder.AppendLine();
            codeBuilder.AppendLine("// Configure the HTTP request pipeline.");
            codeBuilder.AppendLine("if (!app.Environment.IsDevelopment())");
            codeBuilder.AppendLine("{");
            codeBuilder.AppendLine("    app.UseExceptionHandler(\"/Home/Error\");");
            codeBuilder.AppendLine("    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.");
            codeBuilder.AppendLine("    app.UseHsts();");
            codeBuilder.AppendLine("}");
            codeBuilder.AppendLine();
            codeBuilder.AppendLine("app.UseHttpsRedirection();");
            codeBuilder.AppendLine("app.UseStaticFiles();");
            codeBuilder.AppendLine();
            codeBuilder.AppendLine("app.UseRouting();");

            if (Api == 1 || mvcApi == 1)
            {
                codeBuilder.AppendLine("app.UseCors(\"AllowAllOrigins\");");
            }

            codeBuilder.AppendLine("app.UseAuthentication();");
            codeBuilder.AppendLine("app.UseAuthorization();");
            codeBuilder.AppendLine();
            codeBuilder.AppendLine("app.MapControllerRoute(");
            codeBuilder.AppendLine("    name: \"default\",");



            if (isLoginRequired == 1 && Api == 0)
            {
                codeBuilder.AppendLine("    pattern: \"{controller=Login}/{action=Index}/{id?}\");");
            }
            else
            {
                codeBuilder.AppendLine("    pattern: \"{controller=Home}/{action=Index}/{id?}\");");
            }
            codeBuilder.AppendLine();
            codeBuilder.AppendLine("app.Run();");

            return codeBuilder.ToString();
        }
        public static string GenerateAppSettingsJson()
        {
            StringBuilder codeBuilder = new StringBuilder();

            codeBuilder.AppendLine("{");
            codeBuilder.AppendLine("\"Logging\": {");
            codeBuilder.AppendLine("\"LogLevel\": {");
            codeBuilder.AppendLine(" \"Default\": \"Information\",");
            codeBuilder.AppendLine("\"Microsoft.AspNetCore\": \"Warning\"");
            codeBuilder.AppendLine("}");
            codeBuilder.AppendLine("},");
            codeBuilder.AppendLine("\"Jwt\": {");
            codeBuilder.AppendLine("\"Issuer\": \"https://localhost:44346\",");
            codeBuilder.AppendLine(" \"Audience\": \"https://localhost:44346\",");
            codeBuilder.AppendLine("\"Key\": \"ABCD!!@&1234ABCD!!@&123456783409GHJJK\"");
            codeBuilder.AppendLine("},");
            codeBuilder.AppendLine(" \"AllowedHosts\": \"*\"");
            codeBuilder.AppendLine("}");
            codeBuilder.AppendLine();

            return codeBuilder.ToString();
        }
        public static string GenerateIndexViewCode(List<FeplField> fields, string modelName, FeplProject project)
        {
            StringBuilder codeBuilder = new StringBuilder();

            // Generate view code
            codeBuilder.AppendLine("@model IEnumerable<" + project.name + ".Models." + modelName + ">");
            codeBuilder.AppendLine();
            codeBuilder.AppendLine("@{");
            codeBuilder.AppendLine("    ViewData[\"Title\"] = \"Index\";");
            codeBuilder.AppendLine("    string myurl = Context.Request.Path + Context.Request.HttpContext.Request.QueryString;");

            codeBuilder.AppendLine("}");
            codeBuilder.AppendLine();

            if (fields.Where(o => !string.IsNullOrEmpty(o.foreignKeyId)).Count() > 0)
            {
                codeBuilder.AppendLine($"<form method=\"post\" action=\"~/{modelName}/Index\">");
                codeBuilder.AppendLine("    <div class=\"row\">");
                foreach (FeplField field in fields.Where(o => !string.IsNullOrEmpty(o.foreignKeyId)))
                {
                    codeBuilder.AppendLine("        <div class=\"col-3\">");
                    codeBuilder.AppendLine($"            <label class=\"control-label\">{field.printName}</label>");
                    codeBuilder.AppendLine($"            <select name=\"{field.name}\" id=\"{field.name}\" class=\"form-control\">");
                    codeBuilder.AppendLine($"                <option value=\"\">--Select--</option>");
                    codeBuilder.AppendLine($"                @foreach (var obj in ViewBag.{field.foreignKeyNav?.tableNav?.name})");
                    codeBuilder.AppendLine("                {");
                    codeBuilder.AppendLine($"                    if (ViewBag.{field.name} != null && ViewBag.{field.name} == obj.id)");
                    codeBuilder.AppendLine("                    {");
                    codeBuilder.AppendLine("                        <option value=\"@obj.id\" selected>@obj.name</option>");
                    codeBuilder.AppendLine("                    }");
                    codeBuilder.AppendLine("                    else");
                    codeBuilder.AppendLine("                    {");
                    codeBuilder.AppendLine("                        <option value=\"@obj.id\">@obj.name</option>");
                    codeBuilder.AppendLine("                    }");
                    codeBuilder.AppendLine("                }");
                    codeBuilder.AppendLine("            </select>");
                    codeBuilder.AppendLine("        </div>");
                }
                codeBuilder.AppendLine("<span style=\"margin-top: 15px;\"></span>");
                codeBuilder.AppendLine("        <div class=\"col-3\"><button class=\"btn btn-info\" type=\"submit\">Submit</button></div>");
                codeBuilder.AppendLine("<span style=\"margin-bottom: 15px;\"></span>");
                codeBuilder.AppendLine("    </div>");
                codeBuilder.AppendLine("</form>");
            }

            codeBuilder.AppendLine("<h1>Index</h1>");
            codeBuilder.AppendLine();
            codeBuilder.AppendLine("<p>");

            codeBuilder.AppendLine($"    <a href=\"~/{modelName}/Create?url=@myurl\" class=\"btn btn-success\">Create New</a>");

            //excel and pdf code

            //codeBuilder.AppendLine("<div>");
            string GenerateExcel = "GenerateExcel?";
            string GeneratePdf = "GeneratePdf?";
            if (fields.Where(o => !string.IsNullOrEmpty(o.foreignKeyId)).Count() > 0)
            {
                foreach (FeplField field in fields.Where(o => !string.IsNullOrEmpty(o.foreignKeyId)))
                {
                    GenerateExcel += $"{field.name}=@ViewBag.{field.name}&";
                    GeneratePdf += $"{field.name}=@ViewBag.{field.name}&";

                }

            }
            GenerateExcel = GenerateExcel.Remove(GenerateExcel.Length - 1);
            GeneratePdf = GeneratePdf.Remove(GeneratePdf.Length - 1);

            //codeBuilder.AppendLine($"<a href = \"/{modelName}/{GenerateExcel}\" class=\"btn btn-primary\">DOWNLOAD EXCEL</a>");
            //codeBuilder.AppendLine("<span style = \"margin-right: 20px;\" ></ span >");
            //codeBuilder.AppendLine($"<a href= \"/{modelName}/{GeneratePdf}\" class=\"btn btn-secondary\">DOWNLOAD PDF</a>");
            //codeBuilder.AppendLine("</div>");
            codeBuilder.AppendLine("<span style = \"margin -right:20px;\"></span>");
            codeBuilder.AppendLine($"<a href = \"/{modelName}/{GenerateExcel}\" class=\"btn btn-primary\">");
            codeBuilder.AppendLine("<i class=\"fas fa-file-excel\"></i>");
            codeBuilder.AppendLine("</a>");
            codeBuilder.AppendLine("<span style = \"margin -right:20px;\"></span>");
            codeBuilder.AppendLine($"<a href=\"/{modelName}/{GeneratePdf}\" class=\"btn btn-secondary\">");
            codeBuilder.AppendLine("<i class=\"fas fa-file-pdf\"></i>");
            codeBuilder.AppendLine("</a>");
            codeBuilder.AppendLine("<br />");

            codeBuilder.AppendLine("</p>");
            codeBuilder.AppendLine("<table class=\"table\">");
            codeBuilder.AppendLine("    <thead>");
            codeBuilder.AppendLine("        <tr>");

            foreach (var field in fields.Where(fld => fld.isHidden == 0))
            {
                codeBuilder.AppendLine("            <th>");
                codeBuilder.AppendLine($"                {field.printName}");
                codeBuilder.AppendLine("            </th>");
            }

            codeBuilder.AppendLine("            <th></th>");
            codeBuilder.AppendLine("        </tr>");
            codeBuilder.AppendLine("    </thead>");
            codeBuilder.AppendLine("    <tbody>");
            codeBuilder.AppendLine("@foreach (var item in Model) {");
            codeBuilder.AppendLine("        <tr>");

            foreach (var field in fields.Where(fld => fld.isHidden == 0))
            {
                codeBuilder.AppendLine($"            <td>");

                if (field.dataTypeNav.name == "Boolean")
                {
                    codeBuilder.AppendLine($"                @(item.{field.name}==1?\"Yes\":\"No\")");
                }

                else if (field.dataTypeNav.name == "Date")
                {

                    codeBuilder.AppendLine($"                @(item.{field.name}>0? item.{field.name}.ToDate().ToString(\"dd-MMM-yyyy\"):\"\")");
                }

                else if (string.IsNullOrEmpty(field.foreignKeyId))
                {
                    codeBuilder.AppendLine($"                @item.{field.name}");
                }

                else
                {
                    codeBuilder.AppendLine($"                @item.{field.name.TrimId()}Nav?.name");
                }

                codeBuilder.AppendLine($"            </td>");
            }

            codeBuilder.AppendLine("            <td>");
            codeBuilder.AppendLine("                @Html.ActionLink(\"Edit\", \"Edit\", new { id = item.id , url = myurl}, new { @class = \"btn btn-primary\" }) |");

            codeBuilder.AppendLine("                @Html.ActionLink(\"Delete\", \"Delete\", new { id = item.id , url = myurl }, new { @class = \"btn btn-danger\" })");
            codeBuilder.AppendLine("            </td>");
            codeBuilder.AppendLine("        </tr>");
            codeBuilder.AppendLine("}");
            codeBuilder.AppendLine("    </tbody>");
            codeBuilder.AppendLine("</table>");
            if (fields.Where(o => !string.IsNullOrEmpty(o.foreignKeyId)).Count() > 0)
            {
                codeBuilder.AppendLine("<link rel = \"stylesheet\" href = \"https://cdn.jsdelivr.net/npm/select2@4.0.13/dist/css/select2.min.css\"/>");
                codeBuilder.AppendLine("<script type = \"text / javascript\" src = \"https://ajax.googleapis.com/ajax/libs/jquery/1.8.3/jquery.min.js\" ></script>");
                codeBuilder.AppendLine("<script src = \"https://ajax.googleapis.com/ajax/libs/jquery/1.11.1/jquery.min.js\" ></script>");
                codeBuilder.AppendLine("<script type = \"text / javascript\" src = \"https://cdn.jsdelivr.net/npm/select2@4.0.13/dist/js/select2.min.js\" ></script>");
                codeBuilder.AppendLine("<script type = \"text / javascript\">");
                codeBuilder.AppendLine("$(function() {");
                foreach (FeplField field in fields.Where(o => !string.IsNullOrEmpty(o.foreignKeyId)))
                {
                    codeBuilder.AppendLine($"$(\"#{field.name}\").select2(); ");
                }
                codeBuilder.AppendLine("});");
                codeBuilder.AppendLine("</script>");
            }
            return codeBuilder.ToString();
        }
        public static string GenerateCreateViewCode(List<FeplField> fields, string modelName, FeplProject project)
        {
            StringBuilder codeBuilder = new StringBuilder();

            // Generate view code
            codeBuilder.AppendLine("@model " + project.name + ".Models." + modelName);
            codeBuilder.AppendLine();
            codeBuilder.AppendLine("@{");
            codeBuilder.AppendLine("    ViewData[\"Title\"] = \"Create\";");
            codeBuilder.AppendLine("}");
            codeBuilder.AppendLine();
            codeBuilder.AppendLine("<h1>Create</h1>");
            codeBuilder.AppendLine();
            codeBuilder.AppendLine($"<h4>{modelName}</h4>");
            codeBuilder.AppendLine("<hr />");
            codeBuilder.AppendLine("<div class=\"row\">");
            codeBuilder.AppendLine("    <div class=\"col-md-4\">");
            codeBuilder.AppendLine("        <form asp-action=\"Create\"  enctype=\"multipart/form-data\">");
            codeBuilder.AppendLine("            <div asp-validation-summary=\"ModelOnly\" class=\"text-danger\"></div>");

            List<FeplField> fieldm = FeplField.Get(Utility.FillStyle.WithFullNav);
            foreach (var field in fields.Where(fld => fld.isHidden == 0))
            {
                if (field.dataTypeNav.name == "Boolean")
                {
                    codeBuilder.AppendLine("            <div class=\"form-group\">");
                    codeBuilder.AppendLine($"                <label class=\"control-label\">{field.printName}</label>");
                    if (field.isRequired == 0)
                    {
                        codeBuilder.AppendLine($"                <select name=\"{field.name}\" id=\"{field.name}\" class=\"form-control\" >");
                    }
                    else
                    {
                        codeBuilder.AppendLine($"                <select name=\"{field.name}\" id=\"{field.name}\" required class=\"form-control\" >");
                    }
                    codeBuilder.AppendLine($"                    <option value=\"\">--Select--</option>");
                    codeBuilder.AppendLine($"                    @if(Model.{field.name} == 1)");
                    codeBuilder.AppendLine($"                    {{");
                    codeBuilder.AppendLine($"                    <option value=\"1\" selected>Yes</option>");
                    codeBuilder.AppendLine($"                    <option value=\"0\">No</option>");
                    codeBuilder.AppendLine($"                    }}");
                    codeBuilder.AppendLine($"                    else if(Model.{field.name}==0)");
                    codeBuilder.AppendLine($"                    {{");
                    codeBuilder.AppendLine($"                    <option value=\"1\">Yes</option>");
                    codeBuilder.AppendLine($"                    <option value=\"0\" selected>No</option>");
                    codeBuilder.AppendLine($"                    }}");
                    codeBuilder.AppendLine($"                    else");
                    codeBuilder.AppendLine($"                    {{");
                    codeBuilder.AppendLine($"                    <option value=\"1\">Yes</option>");
                    codeBuilder.AppendLine($"                    <option value=\"0\">No</option>");
                    codeBuilder.AppendLine($"                    }}");
                    codeBuilder.AppendLine($"                </select>");
                    codeBuilder.AppendLine("            </div>");
                }
                else if (field.dataTypeNav.name == "Date")
                {


                    codeBuilder.AppendLine("            <div class=\"form-group\">");
                    codeBuilder.AppendLine($"                <label class=\"control-label\">{field.printName}</label>");

                    codeBuilder.AppendLine($"                <input name=\"{field.name}\" type=\"date\" id=\"{field.name}\" value=\"@DateTime.Now.Date.ToString(\"yyyy-MM-dd\")\" required class=\"form-control\" />");
                    codeBuilder.AppendLine("            </div>");

                }
                else if (field.dataTypeNav.name == "File")
                {
                    codeBuilder.AppendLine("            <div class=\"form-group\">");
                    codeBuilder.AppendLine($"                <label class=\"control-label\">{field.printName}</label>");

                    codeBuilder.AppendLine($"                <input name=\"{field.name + "File"}\" type=\"file\" id=\"{field.name}\" value=\"@Model.{field.name}\" class=\"form-control\"/>");
                    codeBuilder.AppendLine("            </div>");

                }
                else if (string.IsNullOrEmpty(field.foreignKeyId))
                {
                    codeBuilder.AppendLine("            <div class=\"form-group\">");
                    codeBuilder.AppendLine($"                <label class=\"control-label\">{field.printName}</label>");
                    if (field.isRequired == 0)
                    {
                        codeBuilder.AppendLine($"                <input name=\"{field.name}\" id=\"{field.name}\" value=\"@Model.{field.name}\" class=\"form-control\" />");
                    }
                    else
                    {
                        codeBuilder.AppendLine($"                <input name=\"{field.name}\" id=\"{field.name}\" value=\"@Model.{field.name}\" required class=\"form-control\" />");
                    }
                    codeBuilder.AppendLine("            </div>");
                }
                else
                {
                    codeBuilder.AppendLine("            <div class=\"form-group\">");
                    codeBuilder.AppendLine($"                <label class=\"control-label\">{field.printName}</label>");
                    if (field.isRequired == 0)
                    {
                        codeBuilder.AppendLine($"                <select name=\"{field.name}\" id=\"{field.name}\" class=\"form-control\" >");
                    }
                    else
                    {
                        codeBuilder.AppendLine($"                <select name=\"{field.name}\" id=\"{field.name}\" required class=\"form-control\" >");
                    }
                    codeBuilder.AppendLine($"                    <option value=\"\">--Select--</option>");
                    codeBuilder.AppendLine($"                    @foreach(var obj in ViewBag.{field.foreignKeyNav?.tableNav?.name})");
                    codeBuilder.AppendLine($"                     {{");

                    codeBuilder.AppendLine($"                 @if (Model.{field.name} == obj.id)");
                    codeBuilder.AppendLine($"                {{");
                    codeBuilder.AppendLine($"                <option value=\"@obj.id\" selected>@obj.name</option>");
                    codeBuilder.AppendLine($"                }}");
                    codeBuilder.AppendLine($"                else");
                    codeBuilder.AppendLine($"                {{");
                    codeBuilder.AppendLine($"                <option value=\"@obj.id\">@obj.name</option>");
                    codeBuilder.AppendLine($"                }}");
                    codeBuilder.AppendLine($"                     }}");
                    codeBuilder.AppendLine($"                </select>");
                    codeBuilder.AppendLine("            </div>");
                }
            }
            if (fields.Where(a => a.metaRequired == 1).Count() > 0)
            {
                codeBuilder.AppendLine("<div class=\"row\" style=\"margin-top:20px;\" id=\"MultipleDataTable\">");
                codeBuilder.AppendLine($"<input asp-for=\"{fields.Where(a => a.metaRequired == 1).FirstOrDefault().printName.ToLower()}\" id=\"metaArray\" class=\"form-control\" hidden=\"hidden\"/>");
                codeBuilder.AppendLine("<div class=\"container - fluid\">");
                codeBuilder.AppendLine("<div class=\"jumbotron\">");
                codeBuilder.AppendLine("      <div class=\"row\">");
                foreach (var fieldi in fieldm.Where(o => o.tableId == fields.Where(a => a.metaRequired == 1).FirstOrDefault().metaTableId.ToString() && o.isHidden == 0))
                {
                    codeBuilder.AppendLine("           <div class=\"col-lg-6\">");
                    codeBuilder.AppendLine($"                 <label class=\"control - label\">{fieldi.name}</label>");
                    codeBuilder.AppendLine($"                 <input type = \"text\" id={fieldi.name} name={fieldi.name} style=\"text-transform:uppercase; \" placeholder={fieldi.name} class=\"form-control\"/>");
                    //codeBuilder.AppendLine("                 <label class=\"control - label\">Order Meta Name</label>");
                    //codeBuilder.AppendLine("                 <input type = \"text\" id=\"OrderMetaName\" name=\"OrderMetaName\" style=\"text - transform:uppercase; \" placeholder=\"OrderMetaName\" class=\"form - control\" />");
                    codeBuilder.AppendLine("             </div>");
                }
                codeBuilder.AppendLine("             <div class=\"col-lg-6\" style=\"margin-top:32px;\">");
                codeBuilder.AppendLine("                 <center>");
                codeBuilder.AppendLine("                     <a id = \"addToList\" class=\"btn btn-primary\" style=\"color:#fff\" onclick=\"addRow()\">Add</a>");
                codeBuilder.AppendLine("                 </center>");
                codeBuilder.AppendLine("             </div>");
                codeBuilder.AppendLine("         </div>");
                codeBuilder.AppendLine("         <br/>");
                codeBuilder.AppendLine("         <table class=\"table table-responsive\" id=\"dataTable\">");
                codeBuilder.AppendLine("             <thead>");
                codeBuilder.AppendLine("                 <tr>");
                //codeBuilder.AppendLine("                     <th hidden = \"hidden\"> Id </ th >");
                foreach (var fieldi in fieldm.Where(o => o.tableId == fields.Where(a => a.metaRequired == 1).FirstOrDefault().metaTableId && o.isHidden == 0))
                {
                    codeBuilder.AppendLine($"  <th> {fieldi.name}</th>");
                }
                //codeBuilder.AppendLine("                     < th > Order Meta Name</th>");
                codeBuilder.AppendLine("                     <th>Edit</th>");
                codeBuilder.AppendLine("                     <th>Delete</th>");
                codeBuilder.AppendLine("                 </tr>");
                codeBuilder.AppendLine("             </thead>");
                codeBuilder.AppendLine("             <tbody></tbody>");
                codeBuilder.AppendLine("         </table>");
                codeBuilder.AppendLine("     </div>");
                codeBuilder.AppendLine(" </div>");
                codeBuilder.AppendLine("</div>");
            }
            codeBuilder.AppendLine("            <input name = \"url\" hidden value = \"@ViewBag.url\" />");
            codeBuilder.AppendLine("            <div class=\"form-group\">");
            codeBuilder.AppendLine("                <input type=\"submit\" id =\"saveData\" value=\"Create\" class=\"btn btn-primary\" />");
            codeBuilder.AppendLine("            </div>");
            codeBuilder.AppendLine("        </form>");
            codeBuilder.AppendLine("    </div>");
            codeBuilder.AppendLine("</div>");
            codeBuilder.AppendLine();
            codeBuilder.AppendLine("<div>");
            codeBuilder.AppendLine("    <a href=\"@ViewBag.url\">Back to List</a>");
            codeBuilder.AppendLine("</div>");
            codeBuilder.AppendLine();
            codeBuilder.AppendLine("@section Scripts {");
            codeBuilder.AppendLine("    @{await Html.RenderPartialAsync(\"_ValidationScriptsPartial\");}");
            codeBuilder.AppendLine("}");
            codeBuilder.AppendLine("<script src=\"https://ajax.googleapis.com/ajax/libs/jquery/3.5.1/jquery.min.js\"></script>\r\n");
            if (fields.Where(o => !string.IsNullOrEmpty(o.foreignKeyId)).Count() > 0)
            {
                codeBuilder.AppendLine("<link rel = \"stylesheet\" href = \"https://cdn.jsdelivr.net/npm/select2@4.0.13/dist/css/select2.min.css\" />");
                codeBuilder.AppendLine("<script type = \"text / javascript\" src = \"https://ajax.googleapis.com/ajax/libs/jquery/1.8.3/jquery.min.js\" ></script>");
                codeBuilder.AppendLine("<script src = \"https://ajax.googleapis.com/ajax/libs/jquery/1.11.1/jquery.min.js\" ></script>");
                codeBuilder.AppendLine("<script type = \"text / javascript\" src = \"https://cdn.jsdelivr.net/npm/select2@4.0.13/dist/js/select2.min.js\" ></script>");
                codeBuilder.AppendLine("<script type = \"text / javascript\" >");
                codeBuilder.AppendLine("$(function() {");
                foreach (FeplField field in fields.Where(o => !string.IsNullOrEmpty(o.foreignKeyId)))
                {
                    codeBuilder.AppendLine($"$(\"#{field.name}\").select2(); ");
                }
                codeBuilder.AppendLine("});");
                codeBuilder.AppendLine("</script>");
            }
            if (fields.Where(o => o.metaRequired == 1).Count() > 0)
            {
                codeBuilder.AppendLine("<script>");

                codeBuilder.AppendLine("function addRow() {");
                foreach (var field in fieldm.Where(o => o.tableId == fields.Where(a => a.metaRequired == 1).FirstOrDefault().metaTableId && o.isHidden == 0))
                {
                    codeBuilder.AppendLine($" var {field.name} = $('#{field.name}').val();");
                }
                codeBuilder.AppendLine("var table = $('#dataTable');");

                codeBuilder.AppendLine("var row = $('<tr>');");

                codeBuilder.AppendLine("var cell1 = $('<td style=\"display: none;\">').appendTo(row);");
                int j = 2;
                foreach (var field in fieldm.Where(o => o.tableId == fields.Where(a => a.metaRequired == 1).FirstOrDefault().metaTableId && o.isHidden == 0))
                {
                    codeBuilder.AppendLine($"var cell{j} = $('<td>').text({field.name}).appendTo(row);");
                    j++;
                }

                codeBuilder.AppendLine($" var cell{j} = $('<td>').html('<button class=\"btn-sm btn-warning\" onclick=\"editRow(this)\">Edit</button>').appendTo(row);");
                codeBuilder.AppendLine($" var cell{j + 1} = $('<td>').html('<button class=\"btn-sm btn-danger\" onclick=\"deleteRow(this)\">Delete</button>').appendTo(row);");

                codeBuilder.AppendLine(" cell1.appendTo(row);");


                codeBuilder.AppendLine("row.appendTo(table);");


                codeBuilder.AppendLine(" clearForm();");
                codeBuilder.AppendLine(" }");

                //codeBuilder.AppendLine("function clearForm()");
                //codeBuilder.AppendLine("{");
                //foreach (var field in fieldm.Where(o => o.tableId == fields.Where(a => a.metaRequired == 1).FirstOrDefault().metaTableId))
                //{
                //    codeBuilder.AppendLine($"$'#{field.name}').val('');");
                //}
                //codeBuilder.AppendLine("    }");





                codeBuilder.AppendLine("function editRow(button)");
                codeBuilder.AppendLine("{");

                codeBuilder.AppendLine("var row = $(button).closest('tr');");

                codeBuilder.AppendLine("var cells = row.find('td');");
                int p = 0;
                foreach (var field in fieldm.Where(o => o.tableId == fields.Where(a => a.metaRequired == 1).FirstOrDefault().metaTableId && o.isHidden == 0))
                {
                    codeBuilder.AppendLine($"$('#{field.name}').val(cells.eq({p}).text());");
                    p++;
                }
                // $('#Designation').val(cells.eq(1).text());


                codeBuilder.AppendLine("row.remove();");

                codeBuilder.AppendLine("}");



                codeBuilder.AppendLine("function deleteRow(button)");
                codeBuilder.AppendLine(" {");
                codeBuilder.AppendLine("     var row = button.parentNode.parentNode;");
                codeBuilder.AppendLine("     row.parentNode.removeChild(row);");
                codeBuilder.AppendLine(" }");

                codeBuilder.AppendLine(" function clearForm()");
                codeBuilder.AppendLine(" {");
                foreach (var field in fieldm.Where(o => o.tableId == fields.Where(a => a.metaRequired == 1).FirstOrDefault().metaTableId && o.isHidden == 0))
                {
                    codeBuilder.AppendLine($"     document.getElementById('{field.name}').value = '';");
                }
                codeBuilder.AppendLine(" }");

                codeBuilder.AppendLine("$(\"#saveData\").click(function(e) {");
                codeBuilder.AppendLine("e.preventDefault();");

                codeBuilder.AppendLine("var json = [];");
                codeBuilder.AppendLine("$('#dataTable').find('tbody tr').each(function() {");

                codeBuilder.AppendLine("var obj = { };");
                codeBuilder.AppendLine("var $td = $(this).find('td');");
                codeBuilder.AppendLine("var key, val;");
                int b = 0;
                foreach (var field in fieldm.Where(o => o.tableId == fields.Where(a => a.metaRequired == 1).FirstOrDefault().metaTableId && o.isHidden == 0))
                {
                    codeBuilder.AppendLine($"key = '{field.name}';");
                    codeBuilder.AppendLine($"val = $td.eq({b}).text();");
                    codeBuilder.AppendLine(" obj[key] = val;");
                    b++;
                }
                codeBuilder.AppendLine("json.push(obj);");
                codeBuilder.AppendLine("    });");



                codeBuilder.AppendLine("var data = JSON.stringify({");
                foreach (var field in fields.Where(o => o.isHidden == 0))
                {
                    codeBuilder.AppendLine($"{field.name}: $('#{field.name}').val(),");
                }
                codeBuilder.AppendLine($"{fields.Where(o => o.metaRequired == 1).FirstOrDefault().printName.ToLower()}: json");
                codeBuilder.AppendLine("});");

                //codeBuilder.AppendLine(" $.when(saveData(data)).then(function(response) {");
                //codeBuilder.AppendLine("console.log(response);");
                //codeBuilder.AppendLine("}).fail(function(err) {");
                //codeBuilder.AppendLine("    console.log(err);");
                //codeBuilder.AppendLine("});");
                codeBuilder.AppendLine(" saveData(data);");
                codeBuilder.AppendLine("});");


                codeBuilder.AppendLine("function saveData(data)");
                codeBuilder.AppendLine("{");
                codeBuilder.AppendLine("    var formData = new FormData();");
                codeBuilder.AppendLine("    formData.append('content', data);");

                codeBuilder.AppendLine("$.ajax({");

                codeBuilder.AppendLine("type: 'POST',");

                codeBuilder.AppendLine($"url: '/{modelName}/Create',");

                codeBuilder.AppendLine("data: formData,");

                codeBuilder.AppendLine("contentType: false,");

                codeBuilder.AppendLine("processData: false,");

                codeBuilder.AppendLine("success: function(data) {");
                codeBuilder.AppendLine("if (data.status == \"ok\")");
                codeBuilder.AppendLine("{");
                codeBuilder.AppendLine($"    location.href = \"/{modelName}/Create\";");
                codeBuilder.AppendLine("}");
                codeBuilder.AppendLine("   }");
                codeBuilder.AppendLine("});");
                codeBuilder.AppendLine("}");


                codeBuilder.AppendLine("</script>");
                //codeBuilder.AppendLine("}");

            }
            return codeBuilder.ToString();
        }
        public static string GenerateEditViewCode(List<FeplField> fields, string modelName, FeplProject project)
        {
            StringBuilder codeBuilder = new StringBuilder();

            // Generate view code
            codeBuilder.AppendLine("@model " + project.name + ".Models." + modelName);
            codeBuilder.AppendLine();
            codeBuilder.AppendLine("@{");
            codeBuilder.AppendLine("    ViewData[\"Title\"] = \"Edit\";");
            codeBuilder.AppendLine("}");
            codeBuilder.AppendLine();
            codeBuilder.AppendLine("<h1>Edit</h1>");
            codeBuilder.AppendLine();
            codeBuilder.AppendLine($"<h4>{modelName}</h4>");
            codeBuilder.AppendLine("<hr />");
            codeBuilder.AppendLine("<div class=\"row\">");
            codeBuilder.AppendLine("    <div class=\"col-md-4\">");
            codeBuilder.AppendLine("        <form asp-action=\"Edit\"  enctype=\"multipart/form-data\">");
            codeBuilder.AppendLine("            <div asp-validation-summary=\"ModelOnly\" class=\"text-danger\"></div>");

            foreach (var field in fields.Where(fld => fld.isHidden == 1))
            {
                codeBuilder.AppendLine($"                <input hidden asp-for=\"{field.name}\" class=\"form-control\" />");
            }
            List<FeplField> fieldm = FeplField.Get(Utility.FillStyle.WithFullNav);
            foreach (var field in fields.Where(fld => fld.isHidden == 0))
            {
                if (field.dataTypeNav.name == "Boolean")
                {
                    codeBuilder.AppendLine("            <div class=\"form-group\">");
                    codeBuilder.AppendLine($"                <label class=\"control-label\">{field.printName}</label>");
                    if (field.isRequired == 0)
                    {
                        codeBuilder.AppendLine($"                <select name=\"{field.name}\" id=\"{field.name}\" class=\"form-control\" >");
                    }
                    else
                    {
                        codeBuilder.AppendLine($"                <select name=\"{field.name}\" id=\"{field.name}\" required class=\"form-control\" >");
                    }
                    codeBuilder.AppendLine($"                    <option value=\"\">--Select--</option>");
                    codeBuilder.AppendLine($"                    @if(Model.{field.name} == 1)");
                    codeBuilder.AppendLine($"                    {{");
                    codeBuilder.AppendLine($"                    <option value=\"1\" selected>Yes</option>");
                    codeBuilder.AppendLine($"                    <option value=\"0\">No</option>");
                    codeBuilder.AppendLine($"                    }}");
                    codeBuilder.AppendLine($"                    else if(Model.{field.name}==0)");
                    codeBuilder.AppendLine($"                    {{");
                    codeBuilder.AppendLine($"                    <option value=\"1\">Yes</option>");
                    codeBuilder.AppendLine($"                    <option value=\"0\" selected>No</option>");
                    codeBuilder.AppendLine($"                    }}");
                    codeBuilder.AppendLine($"                    else");
                    codeBuilder.AppendLine($"                    {{");
                    codeBuilder.AppendLine($"                    <option value=\"1\">Yes</option>");
                    codeBuilder.AppendLine($"                    <option value=\"0\">No</option>");
                    codeBuilder.AppendLine($"                    }}");
                    codeBuilder.AppendLine($"                </select>");
                    codeBuilder.AppendLine("            </div>");
                }
                else if (field.dataTypeNav.name == "Date")
                {


                    codeBuilder.AppendLine("            <div class=\"form-group\">");
                    codeBuilder.AppendLine($"                <label class=\"control-label\">{field.printName}</label>");

                    codeBuilder.AppendLine($"                <input name=\"{field.name}\" type=\"date\" id=\"{field.name}\" value=\"@Model.{field.name}.ToDate().ToString(\"yyyy-MM-dd\")\" required class=\"form-control\" />");
                    codeBuilder.AppendLine("            </div>");

                }
                else if (field.dataTypeNav.name == "File")
                {
                    codeBuilder.AppendLine("            <div class=\"form-group\">");
                    codeBuilder.AppendLine($"                <label class=\"control-label\">{field.printName}</label>");
                    codeBuilder.AppendLine($"           @if(Model.{field.name} != null)");
                    codeBuilder.AppendLine($"                    {{");
                    codeBuilder.AppendLine($"               <iframe src = \"~/Image/@Model.{field.name}\" width =\"100%\" height =\"400px\" ></iframe >");
                    codeBuilder.AppendLine($"                    }}");
                    codeBuilder.AppendLine($"                <input name=\"{field.name + "File"}\" type=\"file\" id=\"{field.name + "File"}\"  class=\"form-control\" />");
                    codeBuilder.AppendLine($"                <input hidden asp-for=\"{field.name}\" class=\"form-control\" />");
                    codeBuilder.AppendLine("            </div>");
                }
                else if (string.IsNullOrEmpty(field.foreignKeyId))
                {
                    codeBuilder.AppendLine("            <div class=\"form-group\">");
                    codeBuilder.AppendLine($"                <label class=\"control-label\">{field.printName}</label>");
                    if (field.isRequired == 0)
                    {
                        codeBuilder.AppendLine($"                <input name=\"{field.name}\" id=\"{field.name}\" value=\"@Model.{field.name}\" class=\"form-control\" />");
                    }
                    else
                    {
                        codeBuilder.AppendLine($"                <input name=\"{field.name}\" id=\"{field.name}\" value=\"@Model.{field.name}\" required class=\"form-control\" />");
                    }
                    codeBuilder.AppendLine("            </div>");
                }
                else
                {
                    codeBuilder.AppendLine("            <div class=\"form-group\">");
                    codeBuilder.AppendLine($"                <label class=\"control-label\">{field.printName}</label>");
                    if (field.isRequired == 0)
                    {
                        codeBuilder.AppendLine($"                <select name=\"{field.name}\" id=\"{field.name}\" class=\"form-control\" >");
                    }
                    else
                    {
                        codeBuilder.AppendLine($"                <select name=\"{field.name}\" id=\"{field.name}\" required class=\"form-control\" >");
                    }
                    codeBuilder.AppendLine($"                    <option value=\"\">--Select--</option>");
                    codeBuilder.AppendLine($"                    @foreach(var obj in ViewBag.{field.foreignKeyNav?.tableNav?.name})");
                    codeBuilder.AppendLine($"                     {{");

                    codeBuilder.AppendLine($"                 @if (Model.{field.name} == obj.id)");
                    codeBuilder.AppendLine($"                {{");
                    codeBuilder.AppendLine($"                <option value=\"@obj.id\" selected>@obj.name</option>");
                    codeBuilder.AppendLine($"                }}");
                    codeBuilder.AppendLine($"                else");
                    codeBuilder.AppendLine($"                {{");
                    codeBuilder.AppendLine($"                <option value=\"@obj.id\">@obj.name</option>");
                    codeBuilder.AppendLine($"                }}");



                    //codeBuilder.AppendLine($"                       <option value=\"@obj.id\" selected=\"@((Model != null && Model.{field.feplFieldNav?.name} == obj.id).ToString().ToLower())\">@obj.name</option>");

                    codeBuilder.AppendLine($"                     }}");
                    codeBuilder.AppendLine($"                </select>");
                    codeBuilder.AppendLine("            </div>");
                }
            }
            if (fields.Where(a => a.metaRequired == 1).Count() > 0)
            {
                codeBuilder.AppendLine("<div class=\"row\" style=\"margin-top:20px;\" id=\"MultipleDataTable\">");
                codeBuilder.AppendLine($"<input asp-for=\"{fields.Where(a => a.metaRequired == 1).FirstOrDefault().printName.ToLower()}\" id=\"metaArray\" class=\"form-control\" hidden=\"hidden\"/>");
                codeBuilder.AppendLine("<div class=\"container - fluid\">");
                codeBuilder.AppendLine("<div class=\"jumbotron\">");
                codeBuilder.AppendLine("      <div class=\"row\">");
                foreach (var fieldi in fieldm.Where(o => o.tableId == fields.Where(a => a.metaRequired == 1).FirstOrDefault().metaTableId.ToString() && o.isHidden == 0))
                {
                    codeBuilder.AppendLine("           <div class=\"col-lg-6\">");
                    codeBuilder.AppendLine($"                 <label class=\"control - label\">{fieldi.name}</label>");
                    codeBuilder.AppendLine($"                 <input type = \"text\" id={fieldi.name} name={fieldi.name} style=\"text-transform:uppercase; \" placeholder={fieldi.name} class=\"form-control\"/>");
                    //codeBuilder.AppendLine("                 <label class=\"control - label\">Order Meta Name</label>");
                    //codeBuilder.AppendLine("                 <input type = \"text\" id=\"OrderMetaName\" name=\"OrderMetaName\" style=\"text - transform:uppercase; \" placeholder=\"OrderMetaName\" class=\"form - control\" />");
                    codeBuilder.AppendLine("             </div>");
                }
                codeBuilder.AppendLine("             <div class=\"col-lg-6\" style=\"margin-top:32px;\">");
                codeBuilder.AppendLine("                 <center>");
                codeBuilder.AppendLine("                     <a id = \"addToList\" class=\"btn btn-primary\" style=\"color:#fff\" onclick=\"addRow()\">Add</a>");
                codeBuilder.AppendLine("                 </center>");
                codeBuilder.AppendLine("             </div>");
                codeBuilder.AppendLine("         </div>");
                codeBuilder.AppendLine("         <br/>");
                codeBuilder.AppendLine("         <table class=\"table table-responsive\" id=\"dataTable\">");
                codeBuilder.AppendLine("             <thead>");
                codeBuilder.AppendLine("                 <tr>");
                //codeBuilder.AppendLine("                     <th hidden = \"hidden\"> Id </ th >");
                foreach (var fieldi in fieldm.Where(o => o.tableId == fields.Where(a => a.metaRequired == 1).FirstOrDefault().metaTableId && o.isHidden == 0))
                {
                    codeBuilder.AppendLine($"  <th> {fieldi.name}</th>");
                }
                //codeBuilder.AppendLine("                     < th > Order Meta Name</th>");
                codeBuilder.AppendLine("                     <th>Edit</th>");
                codeBuilder.AppendLine("                     <th>Delete</th>");
                codeBuilder.AppendLine("                 </tr>");
                codeBuilder.AppendLine("             </thead>");
                codeBuilder.AppendLine("             <tbody>");
                codeBuilder.AppendLine($"             @foreach (var item in Model.{fields.Where(a => a.metaRequired == 1).FirstOrDefault().printName.ToLower()})");
                codeBuilder.AppendLine("             {");
                codeBuilder.AppendLine("             <tr>");
                foreach (var fieldi in fieldm.Where(o => o.tableId == fields.Where(a => a.metaRequired == 1).FirstOrDefault().metaTableId && o.isHidden == 0))
                {
                    codeBuilder.AppendLine($"             <td class={fieldi.name}>@item.{fieldi.name}</td>");
                }
                codeBuilder.AppendLine("             <td><button class=\"btn-sm btn-warning\" onclick=\"editRow(this)\">Edit</button></td>");
                codeBuilder.AppendLine("             <td><button class=\"btn-sm btn-danger\" onclick=\"deleteRow(this)\">Delete</button></td>");
                codeBuilder.AppendLine("             </tr>");
                codeBuilder.AppendLine("             }");
                codeBuilder.AppendLine("             </tbody>");
                codeBuilder.AppendLine("         </table>");
                codeBuilder.AppendLine("     </div>");
                codeBuilder.AppendLine(" </div>");
                codeBuilder.AppendLine("</div>");
            }
            codeBuilder.AppendLine("            <input name = \"url\" hidden value = \"@ViewBag.url\" />");
            codeBuilder.AppendLine("            <div class=\"form-group\">");
            codeBuilder.AppendLine("                <input type=\"submit\" id=\"saveData\" value=\"Save\" class=\"btn btn-primary\" />");
            codeBuilder.AppendLine("            </div>");
            codeBuilder.AppendLine("        </form>");
            codeBuilder.AppendLine("    </div>");
            codeBuilder.AppendLine("</div>");
            codeBuilder.AppendLine();
            codeBuilder.AppendLine("<div>");
            codeBuilder.AppendLine("    <a href=\"@ViewBag.url\">Back to List</a>");
            codeBuilder.AppendLine("</div>");
            codeBuilder.AppendLine();
            codeBuilder.AppendLine("@section Scripts {");
            codeBuilder.AppendLine("    @{await Html.RenderPartialAsync(\"_ValidationScriptsPartial\");}");
            codeBuilder.AppendLine("}");
            codeBuilder.AppendLine("<script src=\"https://ajax.googleapis.com/ajax/libs/jquery/3.5.1/jquery.min.js\"></script>\r\n");
            if (fields.Where(o => !string.IsNullOrEmpty(o.foreignKeyId)).Count() > 0)
            {
                codeBuilder.AppendLine("<link rel = \"stylesheet\" href = \"https://cdn.jsdelivr.net/npm/select2@4.0.13/dist/css/select2.min.css\"/>");
                codeBuilder.AppendLine("<script type = \"text / javascript\" src = \"https://ajax.googleapis.com/ajax/libs/jquery/1.8.3/jquery.min.js\" ></script>");
                codeBuilder.AppendLine("<script src = \"https://ajax.googleapis.com/ajax/libs/jquery/1.11.1/jquery.min.js\"></script>");
                codeBuilder.AppendLine("<script type = \"text / javascript\" src = \"https://cdn.jsdelivr.net/npm/select2@4.0.13/dist/js/select2.min.js\" ></script>");
                codeBuilder.AppendLine("<script type = \"text / javascript\">");
                codeBuilder.AppendLine("$(function() {");
                foreach (FeplField field in fields.Where(o => !string.IsNullOrEmpty(o.foreignKeyId)))
                {
                    codeBuilder.AppendLine($"$(\"#{field.name}\").select2(); ");
                }
                codeBuilder.AppendLine("});");
                codeBuilder.AppendLine("</script>");
            }
            if (fields.Where(o => o.metaRequired == 1).Count() > 0)
            {
                codeBuilder.AppendLine("<script>");

                codeBuilder.AppendLine("function addRow() {");
                foreach (var field in fieldm.Where(o => o.tableId == fields.Where(a => a.metaRequired == 1).FirstOrDefault().metaTableId && o.isHidden == 0))
                {
                    codeBuilder.AppendLine($" var {field.name} = $('#{field.name}').val();");
                }
                codeBuilder.AppendLine("var table = $('#dataTable');");

                codeBuilder.AppendLine("var row = $('<tr>');");

                codeBuilder.AppendLine("var cell1 = $('<td style=\"display: none;\">').appendTo(row);");
                int j = 2;
                foreach (var field in fieldm.Where(o => o.tableId == fields.Where(a => a.metaRequired == 1).FirstOrDefault().metaTableId && o.isHidden == 0))
                {
                    codeBuilder.AppendLine($"var cell{j} = $('<td>').text({field.name}).appendTo(row);");
                    j++;
                }

                codeBuilder.AppendLine($" var cell{j} = $('<td>').html('<button class=\"btn-sm btn-warning\" onclick=\"editRow(this)\">Edit</button>').appendTo(row);");
                codeBuilder.AppendLine($" var cell{j + 1} = $('<td>').html('<button class=\"btn-sm btn-danger\" onclick=\"deleteRow(this)\">Delete</button>').appendTo(row);");

                codeBuilder.AppendLine(" cell1.appendTo(row);");


                codeBuilder.AppendLine("row.appendTo(table);");


                codeBuilder.AppendLine(" clearForm();");
                codeBuilder.AppendLine(" }");

                //codeBuilder.AppendLine("function clearForm()");
                //codeBuilder.AppendLine("{");
                //foreach (var field in fieldm.Where(o => o.tableId == fields.Where(a => a.metaRequired == 1).FirstOrDefault().metaTableId))
                //{
                //    codeBuilder.AppendLine($"$'#{field.name}').val('');");
                //}
                //codeBuilder.AppendLine("    }");





                codeBuilder.AppendLine("function editRow(button)");
                codeBuilder.AppendLine("{");

                codeBuilder.AppendLine("var row = $(button).closest('tr');");

                codeBuilder.AppendLine("var cells = row.find('td');");
                int p = 0;
                foreach (var field in fieldm.Where(o => o.tableId == fields.Where(a => a.metaRequired == 1).FirstOrDefault().metaTableId && o.isHidden == 0))
                {
                    codeBuilder.AppendLine($"$('#{field.name}').val(cells.eq({p}).text());");
                    p++;
                }
                // $('#Designation').val(cells.eq(1).text());


                codeBuilder.AppendLine("row.remove();");

                codeBuilder.AppendLine("}");



                codeBuilder.AppendLine("function deleteRow(button)");
                codeBuilder.AppendLine(" {");
                codeBuilder.AppendLine("     var row = button.parentNode.parentNode;");
                codeBuilder.AppendLine("     row.parentNode.removeChild(row);");
                codeBuilder.AppendLine(" }");

                codeBuilder.AppendLine(" function clearForm()");
                codeBuilder.AppendLine(" {");
                foreach (var field in fieldm.Where(o => o.tableId == fields.Where(a => a.metaRequired == 1).FirstOrDefault().metaTableId && o.isHidden == 0))
                {
                    codeBuilder.AppendLine($"     document.getElementById('{field.name}').value = '';");
                }
                codeBuilder.AppendLine(" }");

                codeBuilder.AppendLine("$(\"#saveData\").click(function(e) {");
                codeBuilder.AppendLine("e.preventDefault();");

                codeBuilder.AppendLine("var json = [];");
                codeBuilder.AppendLine("$('#dataTable').find('tbody tr').each(function() {");

                codeBuilder.AppendLine("var obj = { };");
                codeBuilder.AppendLine("var $td = $(this).find('td');");
                codeBuilder.AppendLine("var key, val;");
                int b = 0;
                foreach (var field in fieldm.Where(o => o.tableId == fields.Where(a => a.metaRequired == 1).FirstOrDefault().metaTableId && o.isHidden == 0))
                {
                    codeBuilder.AppendLine($"key = '{field.name}';");
                    codeBuilder.AppendLine($"val = $td.eq({b}).text();");
                    codeBuilder.AppendLine(" obj[key] = val;");
                    b++;
                }
                codeBuilder.AppendLine("json.push(obj);");
                codeBuilder.AppendLine("    });");



                codeBuilder.AppendLine("var data = JSON.stringify({");
                foreach (var field in fields)
                {
                    codeBuilder.AppendLine($"{field.name}: $('#{field.name}').val(),");
                }
                codeBuilder.AppendLine($"{fields?.Where(o => o.metaRequired == 1)?.FirstOrDefault()?.printName?.ToLower()}: json");
                codeBuilder.AppendLine("});");

                //codeBuilder.AppendLine(" $.when(saveData(data)).then(function(response) {");
                //codeBuilder.AppendLine("console.log(response);");
                //codeBuilder.AppendLine("}).fail(function(err) {");
                //codeBuilder.AppendLine("    console.log(err);");
                //codeBuilder.AppendLine("});");
                codeBuilder.AppendLine(" saveData(data);");
                codeBuilder.AppendLine("});");


                codeBuilder.AppendLine("function saveData(data)");
                codeBuilder.AppendLine("{");
                codeBuilder.AppendLine("    var formData = new FormData();");
                codeBuilder.AppendLine("    formData.append('content', data);");

                codeBuilder.AppendLine("$.ajax({");

                codeBuilder.AppendLine("type: 'POST',");

                codeBuilder.AppendLine($"url: '/{modelName}/Edit',");

                codeBuilder.AppendLine("data: formData,");

                codeBuilder.AppendLine("contentType: false,");

                codeBuilder.AppendLine("processData: false,");

                codeBuilder.AppendLine("success: function(data) {");
                codeBuilder.AppendLine("if (data.status == \"ok\")");
                codeBuilder.AppendLine("{");
                codeBuilder.AppendLine($"    location.href = \"/{modelName}/\";");
                codeBuilder.AppendLine("}");
                codeBuilder.AppendLine("   }");
                codeBuilder.AppendLine("});");
                codeBuilder.AppendLine("}");


                codeBuilder.AppendLine("</script>");
                //codeBuilder.AppendLine("}");

            }
            return codeBuilder.ToString();
        }
        public static string GenerateDeleteViewCode(List<FeplField> fields, string modelName, FeplProject project)
        {
            StringBuilder codeBuilder = new StringBuilder();

            // Generate view code
            codeBuilder.AppendLine("@model " + project.name + ".Models." + modelName);
            codeBuilder.AppendLine();
            codeBuilder.AppendLine("@{");
            codeBuilder.AppendLine("    ViewData[\"Title\"] = \"Delete\";");
            codeBuilder.AppendLine("}");
            codeBuilder.AppendLine("@if (!string.IsNullOrEmpty(ViewBag.error))");
            codeBuilder.AppendLine("{");
            codeBuilder.AppendLine("<div class=\"alert alert-danger\">");
            codeBuilder.AppendLine("@ViewBag.error");
            codeBuilder.AppendLine(" </div>");
            codeBuilder.AppendLine("}");
            codeBuilder.AppendLine();
            codeBuilder.AppendLine("<h1>Delete</h1>");
            codeBuilder.AppendLine();
            codeBuilder.AppendLine("<h3>Are you sure you want to delete this?</h3>");
            codeBuilder.AppendLine("<div>");
            codeBuilder.AppendLine($"    <h4>{modelName}</h4>");
            codeBuilder.AppendLine("    <hr />");
            codeBuilder.AppendLine("    <dl class=\"row\">");

            foreach (var field in fields.Where(fld => fld.isHidden == 0))
            {
                codeBuilder.AppendLine($"        <dt class=\"col-sm-2\">");
                codeBuilder.AppendLine($"           {field.printName}");
                codeBuilder.AppendLine($"        </dt>");
                codeBuilder.AppendLine($"        <dd class=\"col-sm-10\">");

                if (field.dataTypeNav.name == "Boolean")
                {
                    codeBuilder.AppendLine($"            @(Model.{field.name} == 1 ? \"Yes\" : \"No\")");
                }

                else if (field.dataTypeNav.name == "Date")
                {
                    codeBuilder.AppendLine($"            @(Model.{field.name}>0?Model.{field.name}.ToDate().ToString(\"dd-MMM-yyyy\"):\"\")");


                }
                else if (string.IsNullOrEmpty(field.foreignKeyId))
                {
                    codeBuilder.AppendLine($"            @Model.{field.name}");
                }
                else
                {
                    codeBuilder.AppendLine($"            @Model.{field.name.TrimId()}Nav?.name");
                }

                codeBuilder.AppendLine($"        </dd>");
            }

            codeBuilder.AppendLine("    </dl>");
            codeBuilder.AppendLine();
            codeBuilder.AppendLine("    <form asp-action=\"Delete\">");
            codeBuilder.AppendLine("            <input name = \"url\" hidden value = \"@ViewBag.url\" />");
            if (fields.Where(o => o.dataTypeNav.name == "File").Count() > 0)
            {
                codeBuilder.AppendLine($"        <input name=\"{fields.Where(o => o.dataTypeNav.name == "File").FirstOrDefault().name + "File"}\" hidden value=\"@Model.{fields.Where(o => o.dataTypeNav.name == "File").FirstOrDefault().name}\" class=\"btn btn-danger\" /> |");
            }
            codeBuilder.AppendLine("        <input type=\"submit\" value=\"Delete\" class=\"btn btn-danger\" /> |");
            codeBuilder.AppendLine("    <a href=\"@ViewBag.url\">Back to List</a>");
            codeBuilder.AppendLine("    </form>");
            codeBuilder.AppendLine("</div>");

            return codeBuilder.ToString();
        }
        public static string GenerateUtilityClassCode(FeplProject project)
        {
            StringBuilder codeBuilder = new StringBuilder();

            codeBuilder.AppendLine($"namespace {project.name}");
            codeBuilder.AppendLine("{");
            codeBuilder.AppendLine("    public static class Utility");
            codeBuilder.AppendLine("    {");
            codeBuilder.AppendLine("        public static string ConnString = \"Data Source=AppData/database.db; foreign keys= true;\";");
            codeBuilder.AppendLine("         public static string response = \"\";");
            codeBuilder.AppendLine("        public enum FillStyle");
            codeBuilder.AppendLine("        {");
            codeBuilder.AppendLine("            Basic,");
            codeBuilder.AppendLine("            AllProperties,");
            codeBuilder.AppendLine("            WithBasicNav,");
            codeBuilder.AppendLine("            WithFullNav,");
            codeBuilder.AppendLine("            Custom");
            codeBuilder.AppendLine("        }");
            codeBuilder.AppendLine();
            codeBuilder.AppendLine("        public static object CreateDynamicObject(Dictionary<string, string> dictionary)");
            codeBuilder.AppendLine("        {");
            codeBuilder.AppendLine("            IDictionary<string, object> expando = new System.Dynamic.ExpandoObject();");
            codeBuilder.AppendLine("            foreach (var entry in dictionary)");
            codeBuilder.AppendLine("            {");
            codeBuilder.AppendLine("                expando[entry.Key] = entry.Value;");
            codeBuilder.AppendLine("            }");
            codeBuilder.AppendLine("            return expando;");
            codeBuilder.AppendLine("        }");

            codeBuilder.AppendLine();
            codeBuilder.AppendLine("        public static DateTime ToDate(this int date)");
            codeBuilder.AppendLine("        {");
            codeBuilder.AppendLine("            int d = date % 100;");
            codeBuilder.AppendLine("            int m = (date / 100) % 100;");
            codeBuilder.AppendLine("            int y = date / 10000;");
            codeBuilder.AppendLine();
            codeBuilder.AppendLine("            return new DateTime(y, m, d);");
            codeBuilder.AppendLine("        }");
            codeBuilder.AppendLine();
            codeBuilder.AppendLine("        public static DateTime ToTime(this int date)");
            codeBuilder.AppendLine("        {");
            codeBuilder.AppendLine("            int SS = date % 100;");
            codeBuilder.AppendLine("            int MM = (date / 100) % 100;");
            codeBuilder.AppendLine("            int HH = date / 10000;");
            codeBuilder.AppendLine();
            codeBuilder.AppendLine("            return new DateTime(DateTime.Now.Year, 1, 1, HH, MM, SS);");
            codeBuilder.AppendLine("        }");


            codeBuilder.AppendLine("    }");
            codeBuilder.AppendLine("}");

            return codeBuilder.ToString();
        }
    }
}
