using System.Globalization;
using System.Diagnostics;
using Scaffolding.Models;
using System.Data.SQLite;
using System.Data;
using Dapper;
using System.Text;

namespace Scaffolding
{
    public static class Utility
    {
        public static string ConnString = "Data Source=AppData/database.db; foreign keys= true;";

        public enum FillStyle
        {
            Basic,
            AllProperties,
            WithBasicNav,
            WithFullNav,
            Custom
        }

        public static string ToCamelCase(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return char.ToLower(input[0]) + input.Substring(1);
        }

        //public static void ExecuteCommand(string command)
        //{
        //    Process process = new();
        //    ProcessStartInfo processStart = new()
        //    {
        //        FileName = "cmd.exe",
        //        Arguments = "/c " + command,
        //        RedirectStandardOutput = true,
        //        UseShellExecute = false,
        //        CreateNoWindow = true
        //    };

        //    process.StartInfo = processStart;
        //    process.Start();
        //    process.WaitForExit();
        //}

        public static void ExecuteCommand(string command, string workingDirectory)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
            processInfo.WorkingDirectory = workingDirectory;
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;



            using (Process process = Process.Start(processInfo))
            {
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    string errorMessage = process.StandardError.ReadToEnd();
                    throw new Exception($"Command '{command}' failed with error: {errorMessage}");
                }
            }
        }

        public static object CreateDynamicObject(Dictionary<string, string> dictionary)
        {
            IDictionary<string, object> expando = new System.Dynamic.ExpandoObject();
            foreach (var entry in dictionary)
            {
                expando[entry.Key] = entry.Value;
            }
            return expando;
        }

        public static List<FeplField> GetFields(string id)
        {
            List<FeplField> fields = new List<FeplField>();
            using (IDbConnection db = new SQLiteConnection(Utility.ConnString))
            {
                db.Open();

                var query = @"SELECT  ff.*,sft.*,fdt.*, fk.*, ft.*,mt.*  FROM FeplFields ff
                            left join FeplTables as sft on ff.tableId=sft.id
                            left join FeplDataTypes as fdt on ff.dataTypeId= fdt.id 
                            left join FeplFields as fk on ff.foreignKeyId=fk.id 
                            left join FeplTables as ft on fk.tableId=ft.id 
                            left join FeplFields as mt on ff.metaTableId = mt.id
                            where ff.tableId = '" + id + "'";

                fields = db.Query<FeplField, FeplTable, FeplDataType, FeplField, FeplTable,FeplField, FeplField>(
                   query,
                   (field, selftable, datatype, selffield, table, metatable) =>
                   {
                       field.tableNav = selftable;
                       field.dataTypeNav = datatype;
                       field.foreignKeyNav = selffield;
                       if (field.foreignKeyNav != null)
                       {
                           field.foreignKeyNav.tableNav = table;
                       }
                       field.metaKeyNav = metatable;
                       if (field.metaKeyNav != null)
                       {
                           field.metaKeyNav.metaTableNav = table;
                       }

                       return field;
                   },
                    splitOn: "id,id,id"
               ).ToList();
            }
            return fields;
        }

        public static string InsertQuery(string tabelId)
        {
            List<FeplField> fields = GetFields(tabelId);

            var insertColumns = string.Join(", ", fields.Select(f => f.name)); // Excluding primary key columns
            var insertParams = string.Join(", ", fields.Select(f => "@" + f.name)); // Excluding primary key columns

            return $"INSERT INTO {fields.FirstOrDefault()?.tableNav.name} ({insertColumns}) VALUES ({insertParams})";
        }

        public static string UpdateQuery(string tabelId)
        {
            List<FeplField> fields = GetFields(tabelId);

            var updateColumns = string.Join(", ", fields.Where(f => f.isPrimaryKey != 1).Select(f => $"{f.name} = @{f.name}"));

            return $"UPDATE {fields.FirstOrDefault()?.tableNav.name} SET {updateColumns} WHERE id = @id;";
        }

        public static string DeleteQuery(string tabelId)
        {
            List<FeplField> fields = GetFields(tabelId);

            return $"DELETE FROM {fields.FirstOrDefault()?.tableNav.name} WHERE id = @id;";
        }

        public static string MaxIdQuery(string tabelId)
        {
            List<FeplField> fields = GetFields(tabelId);
            return $"select Max(numId) from {fields.FirstOrDefault()?.tableNav.name} where idPrefix = 'F'";
        }

        public static string SelectQuery(string tabelId, FillStyle fillStyle)
        {
            List<FeplField> fields = GetFields(tabelId);

            if (fillStyle == FillStyle.Basic)
            {
                return $"SELECT id,name from {fields.FirstOrDefault()?.tableNav?.name}";
            }
            else if (fillStyle == FillStyle.AllProperties)
            {
                return $"SELECT * from {fields.FirstOrDefault()?.tableNav?.name}";
            }
            else if (fillStyle == FillStyle.WithBasicNav)
            {

                StringBuilder codeBuilder = new StringBuilder();

                codeBuilder.Append("SELECT myBaseTable.*");
                int myCount = 1;
                foreach (FeplField field in fields.Where(fld => !string.IsNullOrEmpty(fld.foreignKeyId)))
                {
                    codeBuilder.Append($",Reff{myCount}.id, Reff{myCount}.name");
                    myCount++;
                }
                codeBuilder.Append($" FROM {fields.FirstOrDefault()?.tableNav?.name} myBaseTable ");

                myCount = 1;
                foreach (FeplField field in fields.Where(fld => !string.IsNullOrEmpty(fld.foreignKeyId)))
                {
                    codeBuilder.Append($"left join {field.foreignKeyNav.tableNav.name} Reff{myCount} on myBaseTable.{field.name}=Reff{myCount}.{field.foreignKeyNav.name} ");
                    myCount++;
                }
                return codeBuilder.ToString();
            }
            else if (fillStyle == FillStyle.WithFullNav)
            {
                StringBuilder codeBuilder = new StringBuilder();

                codeBuilder.Append("SELECT myBaseTable.*");
                int myCount = 1;
                foreach (FeplField field in fields.Where(fld => !string.IsNullOrEmpty(fld.foreignKeyId)))
                {
                    codeBuilder.Append($",Reff{myCount}.*");
                    myCount++;
                }
                codeBuilder.Append($" FROM {fields.FirstOrDefault()?.tableNav?.name} myBaseTable ");
                myCount = 1;
                foreach (FeplField field in fields.Where(fld => !string.IsNullOrEmpty(fld.foreignKeyId)))
                {
                    codeBuilder.Append($"left join {field.foreignKeyNav.tableNav.name} Reff{myCount} on myBaseTable.{field.name}=Reff{myCount}.{field.foreignKeyNav.name} ");
                    myCount++;
                }
                return codeBuilder.ToString();
            }
            else
            {
                return "";
            }
        }

        public static void GenerateDataBase(string folderPath, string dbname)
        {
            Directory.CreateDirectory(folderPath + "\\AppData");

            using (IDbConnection db = new SQLiteConnection($"Data Source= {folderPath}\\AppData\\{dbname}; foreign keys= true;"))
            {
                try
                {
                    db.Execute("select * from city");
                }
                catch (Exception ex)
                {

                }

            }
        }


        public static string TrimId(this string input)
        {
            // Check if the input string ends with "id" and has at least 2 characters
            if (input.EndsWith("Id") && input.Length >= 2)
            {
                // Trim the last 2 characters
                return input.Substring(0, input.Length - 2);
            }
            else
            {
                // Return the original string if it doesn't meet the criteria
                return input;
            }
        }


    }

}
