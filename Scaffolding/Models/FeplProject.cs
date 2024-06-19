using System.Data.SQLite;
using System.Data;
using Dapper;


namespace Scaffolding.Models
{
    public class FeplProject
    {
        public string id { get; set; } = "";
        public string name { get; set; } = "";
        public string aliasName { get; set; } = "";
        public int numId { get; set; } = 0;
        public string idPrefix { get; set; } = "";
        public int isLoginRequired { get; set; } = 0;
        // Additional methods
        public static List<FeplProject> Get(Utility.FillStyle fillStyle = Utility.FillStyle.AllProperties, Dictionary<string, string>? paramList = null)
        {
            using (IDbConnection db = new SQLiteConnection(Utility.ConnString))
            {
                db.Open();
                string whereClause = "1=1";
                if (paramList == null)
                {
                    paramList = new Dictionary<string, string>();
                }
                foreach (var obj in paramList)
                {
                    if (!string.IsNullOrEmpty(obj.Value))
                    {
                        whereClause += " and myBaseTable." + obj.Key + " = @" + obj.Key;
                    }
                }
                string query = "";
                if (fillStyle == Utility.FillStyle.Basic)
                {
                    query = "SELECT id,name from FeplProjects";
                    return db.Query<FeplProject>(query).ToList();
                }
                else if (fillStyle == Utility.FillStyle.AllProperties)
                {
                    query = "SELECT * from FeplProjects";
                    return db.Query<FeplProject>(query).ToList();
                }
                else
                {
                    query = "";
                    return db.Query<FeplProject>(query).ToList();
                }
            }
        }
        public static FeplProject GetById(string id, Utility.FillStyle fillStyle = Utility.FillStyle.Basic)
        {
            using (IDbConnection db = new SQLiteConnection(Utility.ConnString))
            {
                db.Open();
                string query = "";
                if (fillStyle == Utility.FillStyle.Basic)
                {
                    query = "SELECT id,name from FeplProjects WHERE id = @id";
                    return db.Query<FeplProject>(query, new { id = id }).FirstOrDefault() ?? new FeplProject();
                }
                else if (fillStyle == Utility.FillStyle.AllProperties)
                {
                    query = "SELECT * from FeplProjects WHERE id = @id";
                    return db.Query<FeplProject>(query, new { id = id }).FirstOrDefault() ?? new FeplProject();
                }
                else
                {
                    query = "";
                    return db.Query<FeplProject>(query).FirstOrDefault() ?? new FeplProject();
                }
            }
        }
        public bool Save()
        {
            using (IDbConnection db = new SQLiteConnection(Utility.ConnString))
            {
                db.Open();
                using (var transaction = db.BeginTransaction())
                {
                    try
                    {
                        idPrefix = "F";
                        numId = db.ExecuteScalar<int>("select Max(numId) from FeplProjects where idPrefix = 'F'") + 1;
                        id = idPrefix + numId;
                        string sql = "INSERT INTO FeplProjects (id, name, aliasName, numId, idPrefix, isLoginRequired) VALUES (@id, @name, @aliasName, @numId, @idPrefix, @isLoginRequired)";
                        int affectedRows = db.Execute(sql, this, transaction);
                        if (this.isLoginRequired == 1)
                        {
                            numId = db.ExecuteScalar<int>("select Max(numId) from FeplTables where idPrefix = 'F'") + 1;
                            string iid = idPrefix + numId;
                            db.Execute($"INSERT INTO FeplTables (id, projectId, name, modelName, numId, idPrefix) " +
                                $"VALUES ('{iid}', '{this.id}', 'LoginUsers', 'LoginUser', {numId}, 'F')", "", transaction);


                            int nnumId = db.ExecuteScalar<int>("select Max(numId) from FeplFields where idPrefix = 'F'") + 1;
                            string iiid = idPrefix + nnumId;
                            //  string sql2 = "INSERT INTO FeplFields (id, tableId, name, dataTypeId, length, isPrimaryKey, isRequired, numId, idPrefix, isHidden, foreignKeyId, printName, defaultValue, foreignTableId) VALUES (@id, @tableId, @name, @dataTypeId, @length, @isPrimaryKey, @isRequired, @numId, @idPrefix, @isHidden, @foreignKeyId, @printName, @defaultValue, @foreignTableId)";
                            db.Execute($"Insert into FeplFields(id, tableId, name, dataTypeId, length, isPrimaryKey, isRequired, numId, " +
                                $"idPrefix, isHidden, foreignKeyId, printName, defaultValue, foreignTableId) " +
                                $"Values('{iiid}','{iid}','{"id"}','F2',50,1,1,{nnumId},'F',1,null,'','',null)", "", transaction);
                            nnumId = nnumId + 1;
                            iiid = idPrefix + nnumId;
                            db.Execute($"Insert into FeplFields(id, tableId, name, dataTypeId, length, isPrimaryKey, isRequired, numId, " +
                                $"idPrefix, isHidden, foreignKeyId, printName, defaultValue, foreignTableId) V" +
                                $"alues('{iiid}','{iid}','{"numId"}','F1',0,0,1,{nnumId},'F',1,null,'','',null)", "", transaction);
                            nnumId = nnumId + 1;
                            iiid = idPrefix + nnumId;
                            db.Execute($"Insert into FeplFields(id, tableId, name, dataTypeId, length, isPrimaryKey, isRequired, numId, " +
                                $"idPrefix, isHidden, foreignKeyId, printName, defaultValue, foreignTableId) " +
                                $"Values('{iiid}','{iid}','{"idPrefix"}','F2',50,0,1,{nnumId},'F',1,null,'','',null)", "", transaction);

                            nnumId = nnumId + 1;
                            iiid = idPrefix + nnumId;
                            db.Execute($"Insert into FeplFields(id, tableId, name, dataTypeId, length, isPrimaryKey, isRequired, numId, " +
                                $"idPrefix, isHidden, foreignKeyId, printName, defaultValue, foreignTableId) " +
                                $"Values('{iiid}','{iid}','{"name"}','F2',250,0,1,{nnumId},'F',0,null,'User Name','',null)", "", transaction);

                            nnumId = nnumId + 1;
                            iiid = idPrefix + nnumId;
                            db.Execute($"Insert into FeplFields(id, tableId, name, dataTypeId, length, isPrimaryKey, isRequired, numId, " +
                                $"idPrefix, isHidden, foreignKeyId, printName, defaultValue, foreignTableId) " +
                                $"Values('{iiid}','{iid}','{"password"}','F2',250,0,1,{nnumId},'F',0,null,'Password','',null)", "", transaction);

                            int LnumId = db.ExecuteScalar<int>("select Max(numId) from LoginUsers where idPrefix = 'F'");
                            if (LnumId < 1)
                            {
                                db.Execute("INSERT INTO LoginUsers (id, numId, idPrefix, name, password) VALUES ('F1', 1, 'F', 'demo', 'demo@123')", "", transaction);
                            }
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {

                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }
        public bool Update()
        {
            using (IDbConnection db = new SQLiteConnection(Utility.ConnString))
            {
                db.Open();
                using (var transaction = db.BeginTransaction())
                {
                    try
                    {
                        string sql = "UPDATE FeplProjects SET name = @name, aliasName = @aliasName, numId = @numId, idPrefix = @idPrefix, isLoginRequired = @isLoginRequired WHERE id = @id;";
                        int affectedRows = db.Execute(sql, this, transaction);

                        if (this.isLoginRequired == 1)
                        {
                            numId = db.ExecuteScalar<int>("select Max(numId) from FeplTables where idPrefix = 'F'") + 1;
                            string iid = idPrefix + numId;
                            db.Execute($"INSERT INTO FeplTables (id, projectId, name, modelName, numId, idPrefix) " +
                                $"VALUES ('{iid}', '{this.id}', 'LoginUsers', 'LoginUser', {numId}, 'F')", "", transaction);


                            int nnumId = db.ExecuteScalar<int>("select Max(numId) from FeplFields where idPrefix = 'F'") + 1;
                            string iiid = idPrefix + nnumId;
                            //  string sql2 = "INSERT INTO FeplFields (id, tableId, name, dataTypeId, length, isPrimaryKey, isRequired, numId, idPrefix, isHidden, foreignKeyId, printName, defaultValue, foreignTableId) VALUES (@id, @tableId, @name, @dataTypeId, @length, @isPrimaryKey, @isRequired, @numId, @idPrefix, @isHidden, @foreignKeyId, @printName, @defaultValue, @foreignTableId)";
                            db.Execute($"Insert into FeplFields(id, tableId, name, dataTypeId, length, isPrimaryKey, isRequired, numId, " +
                                $"idPrefix, isHidden, foreignKeyId, printName, defaultValue, foreignTableId) " +
                                $"Values('{iiid}','{iid}','{"id"}','F2',50,1,1,{nnumId},'F',1,null,'','',null)", "", transaction);
                            nnumId = nnumId + 1;
                            iiid = idPrefix + nnumId;
                            db.Execute($"Insert into FeplFields(id, tableId, name, dataTypeId, length, isPrimaryKey, isRequired, numId, " +
                                $"idPrefix, isHidden, foreignKeyId, printName, defaultValue, foreignTableId) V" +
                                $"alues('{iiid}','{iid}','{"numId"}','F1',0,0,1,{nnumId},'F',1,null,'','',null)", "", transaction);
                            nnumId = nnumId + 1;
                            iiid = idPrefix + nnumId;
                            db.Execute($"Insert into FeplFields(id, tableId, name, dataTypeId, length, isPrimaryKey, isRequired, numId, " +
                                $"idPrefix, isHidden, foreignKeyId, printName, defaultValue, foreignTableId) " +
                                $"Values('{iiid}','{iid}','{"idPrefix"}','F2',50,0,1,{nnumId},'F',1,null,'','',null)", "", transaction);

                            nnumId = nnumId + 1;
                            iiid = idPrefix + nnumId;
                            db.Execute($"Insert into FeplFields(id, tableId, name, dataTypeId, length, isPrimaryKey, isRequired, numId, " +
                                $"idPrefix, isHidden, foreignKeyId, printName, defaultValue, foreignTableId) " +
                                $"Values('{iiid}','{iid}','{"name"}','F2',250,0,1,{nnumId},'F',0,null,'User Name','',null)", "", transaction);

                            nnumId = nnumId + 1;
                            iiid = idPrefix + nnumId;
                            db.Execute($"Insert into FeplFields(id, tableId, name, dataTypeId, length, isPrimaryKey, isRequired, numId, " +
                                $"idPrefix, isHidden, foreignKeyId, printName, defaultValue, foreignTableId) " +
                                $"Values('{iiid}','{iid}','{"password"}','F2',250,0,1,{nnumId},'F',0,null,'Password','',null)", "", transaction);

                            //db.Execute("INSERT INTO LoginUsers (id, numId, idPrefix, userName, password) VALUES ('F1', 1, 'F', 'demo', 'demo@123')", "", transaction);


                        }

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }
        //public bool Delete()
        public (bool Success, Exception Exception) Delete()
        {
            using (IDbConnection db = new SQLiteConnection(Utility.ConnString))
            {
                db.Open();
                using (var transaction = db.BeginTransaction())
                {
                    try
                    {
                        string sql = "DELETE FROM FeplProjects WHERE id = @id;";
                        int affectedRows = db.Execute(sql, new { id = this.id }, transaction);
                        transaction.Commit();
                        //return true;
                        return (true, null);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        // return false;
                        return (false, ex);
                    }
                }
            }
        }
    }
}