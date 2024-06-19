using System.Data.SQLite;
using System.Data;
using Dapper;


namespace Scaffolding.Models
{
    public class FeplTable
    {
        public string id { get; set; } = "";
        public string projectId { get; set; } = "";
        public FeplProject feplProjectNav { get; set; }
        public string name { get; set; } = "";
        public string modelName { get; set; } = "";
        public int numId { get; set; } = 0;
        public string idPrefix { get; set; } = "";
        // Additional methods
        public static List<FeplTable> Get(Utility.FillStyle fillStyle = Utility.FillStyle.AllProperties, Dictionary<string, string>? paramList = null)
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
                    if (obj.Key == "projectId")
                    {
                        whereClause += " and myBaseTable." + obj.Key + " = @" + obj.Key;
                    }
                    else
                    {
                        whereClause += " and myBaseTable." + obj.Key + " = @" + obj.Key;
                    }
                }
                string query = "";
                if (fillStyle == Utility.FillStyle.Basic)
                {
                    query = "SELECT myBaseTable.id,myBaseTable.name from FeplTables myBaseTable where " + whereClause;
                    return db.Query<FeplTable>(query, Utility.CreateDynamicObject(paramList)).ToList();
                }
                else if (fillStyle == Utility.FillStyle.AllProperties)
                {
                    query = "SELECT myBaseTable.* from FeplTables myBaseTable where " + whereClause;
                    return db.Query<FeplTable>(query, Utility.CreateDynamicObject(paramList)).ToList();
                }
                else if (fillStyle == Utility.FillStyle.WithBasicNav)
                {
                    query = "SELECT myBaseTable.*,FeplProjects.id, FeplProjects.name FROM FeplTables " +
                        " myBaseTable left join FeplProjects on myBaseTable.projectId=FeplProjects.id  where " + whereClause;
                    return db.Query<FeplTable, FeplProject, FeplTable>(query,
                    (myBaseTable, feplproject) =>
                    {
                        myBaseTable.feplProjectNav = feplproject;
                        return myBaseTable;
                    }, Utility.CreateDynamicObject(paramList)).ToList();
                }
                else if (fillStyle == Utility.FillStyle.WithFullNav)
                {
                    query = "SELECT myBaseTable.*,FeplProjects.* FROM FeplTables myBaseTable " +
                        " left join FeplProjects on myBaseTable.projectId=FeplProjects.id  where " + whereClause;
                    return db.Query<FeplTable, FeplProject, FeplTable>(query,
                    (myBaseTable, feplproject) =>
                    {
                        myBaseTable.feplProjectNav = feplproject;
                        return myBaseTable;
                    }, Utility.CreateDynamicObject(paramList)).ToList();
                }
                else
                {
                    query = "";
                    return db.Query<FeplTable>(query).ToList();
                }
            }
        }
        public static FeplTable GetById(string id, Utility.FillStyle fillStyle = Utility.FillStyle.Basic)
        {
            using (IDbConnection db = new SQLiteConnection(Utility.ConnString))
            {
                db.Open();
                string query = "";
                if (fillStyle == Utility.FillStyle.Basic)
                {
                    query = "SELECT id,name from FeplTables WHERE id = @id";
                    return db.Query<FeplTable>(query, new { id = id }).FirstOrDefault() ?? new FeplTable();
                }
                else if (fillStyle == Utility.FillStyle.AllProperties)
                {
                    query = "SELECT * from FeplTables WHERE id = @id";
                    return db.Query<FeplTable>(query, new { id = id }).FirstOrDefault() ?? new FeplTable();
                }
                else if (fillStyle == Utility.FillStyle.WithBasicNav)
                {
                    query = "SELECT myBaseTable.*,FeplProjects.id, FeplProjects.name FROM FeplTables myBaseTable left join FeplProjects on myBaseTable.projectId=FeplProjects.id  WHERE myBaseTable.id = @id";
                    return db.Query<FeplTable, FeplProject, FeplTable>(query,
                    (myBaseTable, feplproject) =>
                    {
                        myBaseTable.feplProjectNav = feplproject;
                        return myBaseTable;
                    }, new { id = id }).FirstOrDefault() ?? new FeplTable();
                }
                else if (fillStyle == Utility.FillStyle.WithFullNav)
                {
                    query = "SELECT myBaseTable.*,FeplProjects.* FROM FeplTables myBaseTable left join FeplProjects on myBaseTable.projectId=FeplProjects.id  WHERE myBaseTable.id = @id";
                    return db.Query<FeplTable, FeplProject, FeplTable>(query,
                    (myBaseTable, feplproject) =>
                    {
                        myBaseTable.feplProjectNav = feplproject;
                        return myBaseTable;
                    }, new { id = id }).FirstOrDefault() ?? new FeplTable();
                }
                else
                {
                    query = "";
                    return db.Query<FeplTable>(query).FirstOrDefault() ?? new FeplTable();
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
                        //     numId = db.ExecuteScalar<int>("select Max(numId) from FeplProjects where idPrefix = 'F'") + 1;
                        numId = db.ExecuteScalar<int>("select Max(numId) from FeplTables where idPrefix = 'F'") + 1;
                        id = idPrefix + numId;
                        string sql = "INSERT INTO FeplTables (id, projectId, name, modelName, numId, idPrefix) VALUES (@id, @projectId, @name, @modelName, @numId, @idPrefix)";
                        int affectedRows = db.Execute(sql, this, transaction);
                        
                        idPrefix = "F";
                       int nnumId = db.ExecuteScalar<int>("select Max(numId) from FeplFields where idPrefix = 'F'") + 1;
                        string iid = idPrefix + nnumId;
                        //  string sql2 = "INSERT INTO FeplFields (id, tableId, name, dataTypeId, length, isPrimaryKey, isRequired, numId, idPrefix, isHidden, foreignKeyId, printName, defaultValue, foreignTableId) VALUES (@id, @tableId, @name, @dataTypeId, @length, @isPrimaryKey, @isRequired, @numId, @idPrefix, @isHidden, @foreignKeyId, @printName, @defaultValue, @foreignTableId)";
                        db.Execute($"Insert into FeplFields(id, tableId, name, dataTypeId, length, isPrimaryKey, isRequired, numId, idPrefix, isHidden, foreignKeyId, printName, defaultValue, foreignTableId) " +
                            $"Values('{iid}','{this.id}','{"id"}','F2',50,1,1,{nnumId},'F',1,null,'','',null)","",transaction);
                         nnumId=nnumId + 1;
                         iid = idPrefix + nnumId;
                        db.Execute($"Insert into FeplFields(id, tableId, name, dataTypeId, length, isPrimaryKey, isRequired, numId, idPrefix, isHidden, foreignKeyId, printName, defaultValue, foreignTableId) " +
                            $"Values('{iid}','{this.id}','{"numId"}','F1',0,0,1,{nnumId},'F',1,null,'','',null)","",transaction);
                        nnumId = nnumId + 1;
                         iid = idPrefix + nnumId;
                        db.Execute($"Insert into FeplFields(id, tableId, name, dataTypeId, length, isPrimaryKey, isRequired, numId, idPrefix, isHidden, foreignKeyId, printName, defaultValue, foreignTableId)" +
                            $" Values('{iid}','{this.id}','{"idPrefix"}','F2',50,0,1,{nnumId},'F',1,null,'','',null)","",transaction);


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
                        string sql = "UPDATE FeplTables SET projectId = @projectId, name = @name, modelName = @modelName, numId = @numId, idPrefix = @idPrefix WHERE id = @id;";
                        int affectedRows = db.Execute(sql, this, transaction);
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
        public bool Delete()
        {
            using (IDbConnection db = new SQLiteConnection(Utility.ConnString))
            {
                db.Open();
                using (var transaction = db.BeginTransaction())
                {
                    try
                    {
                        string sql = "DELETE FROM FeplTables WHERE id = @id;";
                        int affectedRows = db.Execute(sql, new { id = this.id }, transaction);
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
    }
}