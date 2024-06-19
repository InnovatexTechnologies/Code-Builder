using Scaffolding.Models;
using System.Reflection;
using System.Text;

namespace Scaffolding
{
    public static class SqLiteScriptBuilder
    {
        public static string GenerateCreateTableScript(List<FeplField> fields, string tableName)
        {
            StringBuilder scriptBuilder = new StringBuilder();

            // Generate table creation script
            scriptBuilder.Append($"CREATE TABLE {tableName} (");

            // Generate columns
            for (int i = 0; i < fields.Count; i++)
            {
                var field = fields[i];
                scriptBuilder.Append($"{field.name} ");

                string dataType = field.dataTypeNav.sqLite == "Varchar" ? $"VARCHAR({field.length})" : field.dataTypeNav.sqLite;
                scriptBuilder.Append(dataType);
                //if(field.name=="id")
                //{
                //    scriptBuilder.Append(" PrimaryKey ");

                //}

                // If the field is primary key, don't add the NOT NULL constraint
                if (field.isPrimaryKey==0 && field.isRequired==1)
                {
                    scriptBuilder.Append(" NOT NULL");
                }

                
                if (i < (fields.Count-1))
                {
                    scriptBuilder.Append(",");
                }

                scriptBuilder.AppendLine();
            }

            // Generate primary key constraint
            List<string> primaryKeys = fields.Where(f => f.isPrimaryKey==1).Select(f => f.name).ToList();
            if (primaryKeys.Count > 0)
            {
                scriptBuilder.AppendLine($", PRIMARY KEY ({string.Join(", ", primaryKeys)})");
            }

            // Generate foreign key constraints
            foreach (FeplField field in fields)
            {
                if (!string.IsNullOrEmpty(field.foreignKeyNav?.tableNav?.name) && !string.IsNullOrEmpty(field.foreignKeyId))
                {
                    scriptBuilder.AppendLine($"FOREIGN KEY({field.name}) REFERENCES {field.foreignKeyNav?.tableNav?.name}({field.foreignKeyNav?.name})");
                }
            }

            // Close the table creation script
            scriptBuilder.Append(");");

            return scriptBuilder.ToString().Trim();
        }
    }
}
