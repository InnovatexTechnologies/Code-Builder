using System.Data.SQLite;
using System.Data;
using Dapper;


namespace Scaffolding.Models
{
    public class FeplField
    {
        public string id { get; set; } = "";
        public string tableId { get; set; } = "";
        public FeplTable tableNav { get; set; }
        public string name { get; set; } = "";
        public string dataTypeId { get; set; } = "";
        public FeplDataType dataTypeNav { get; set; }
        public int length { get; set; } = 0;
        public int isPrimaryKey { get; set; } = 0;
        public int isRequired { get; set; } = 0;
        public int metaRequired { get; set; } = 0;
        public FeplField metaKeyNav { get; set; }
        public FeplTable metaTableNav { get; set; }
        public int numId { get; set; } = 0;
        public string idPrefix { get; set; } = "";
        public int isHidden { get; set; } = 0;
        public string foreignKeyId { get; set; } = "";
        public string metaTableId { get; set; } = "";
        public FeplField foreignKeyNav { get; set; }
        public string printName { get; set; } = "";
        public string defaultValue { get; set; } = "";
        public string foreignTableId { get; set; } = "";
        public FeplTable foreignTableNav { get; set; }
        // Additional methods
        public static List<FeplField> Get(Utility.FillStyle fillStyle = Utility.FillStyle.AllProperties, Dictionary<string, string>? paramList = null)
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
                    query = "SELECT id,name from FeplFields";
                    return db.Query<FeplField>(query).ToList();
                }
                else if (fillStyle == Utility.FillStyle.AllProperties)
                {
                    query = "SELECT * from FeplFields";
                    return db.Query<FeplField>(query).ToList();
                }
                else if (fillStyle == Utility.FillStyle.WithBasicNav)
                {
                    query = "SELECT myBaseTable.*,Reff1.id, Reff1.name,Reff2.id, Reff2.name,Reff3.id, Reff3.name,Reff4.id, Reff4.name,Reff5.id, Reff5.name " +
                        " FROM FeplFields myBaseTable left join FeplTables Reff1 on myBaseTable.tableId=Reff1.id " +
                        " left join FeplDataTypes Reff2 on myBaseTable.dataTypeId=Reff2.id " +
                        " left join FeplFields Reff3 on myBaseTable.foreignKeyId=Reff3.id " +
                        " left join FeplTables Reff4 on myBaseTable.foreignTableId=Reff4.id " +
                        " left join FeplTables Reff5 on myBaseTable.metaTableId=Reff5.id where " + whereClause;

                    return db.Query<FeplField, FeplTable, FeplDataType, FeplField, FeplTable, FeplTable, FeplField>(query,
                    (myBaseTable, ref1, ref2, ref3, ref4, ref5) =>
                    {
                        myBaseTable.tableNav = ref1;
                        myBaseTable.dataTypeNav = ref2;
                        myBaseTable.foreignKeyNav = ref3;
                        myBaseTable.foreignTableNav = ref4;
                        myBaseTable.metaTableNav = ref5;
                        return myBaseTable;
                    }, Utility.CreateDynamicObject(paramList)).ToList();
                }
                else if (fillStyle == Utility.FillStyle.WithFullNav)
                {
                    query = "SELECT myBaseTable.*,Reff1.*,Reff2.*,Reff3.*,Reff4.* FROM FeplFields myBaseTable left join FeplTables Reff1 on myBaseTable.tableId=Reff1.id left join FeplDataTypes Reff2 on myBaseTable.dataTypeId=Reff2.id left join FeplFields Reff3 on myBaseTable.foreignKeyId=Reff3.id left join FeplTables Reff4 on myBaseTable.foreignTableId=Reff4.id  where " + whereClause;
                    return db.Query<FeplField, FeplTable, FeplDataType, FeplField, FeplTable, FeplField>(query,
                    (myBaseTable, ref1, ref2, ref3, ref4) =>
                    {
                        myBaseTable.tableNav = ref1;
                        myBaseTable.dataTypeNav = ref2;
                        myBaseTable.foreignKeyNav = ref3;
                        myBaseTable.foreignTableNav = ref4;
                        return myBaseTable;
                    }, Utility.CreateDynamicObject(paramList)).ToList();
                }
                else
                {
                    query = "";
                    return db.Query<FeplField>(query).ToList();
                }
            }
        }
        public static FeplField GetById(string id, Utility.FillStyle fillStyle = Utility.FillStyle.Basic)
        {
            using (IDbConnection db = new SQLiteConnection(Utility.ConnString))
            {
                db.Open();
                string query = "";
                if (fillStyle == Utility.FillStyle.Basic)
                {
                    query = "SELECT id,name from FeplFields WHERE id = @id";
                    return db.Query<FeplField>(query, new { id = id }).FirstOrDefault() ?? new FeplField();
                }
                else if (fillStyle == Utility.FillStyle.AllProperties)
                {
                    query = "SELECT * from FeplFields WHERE id = @id";
                    return db.Query<FeplField>(query, new { id = id }).FirstOrDefault() ?? new FeplField();
                }
                else if (fillStyle == Utility.FillStyle.WithBasicNav)
                {
                    query = "SELECT myBaseTable.*,Reff1.id, Reff1.name,Reff2.id, Reff2.name,Reff3.id, Reff3.name,Reff4.id, Reff4.name FROM FeplFields myBaseTable left join FeplTables Reff1 on myBaseTable.tableId=Reff1.id left join FeplDataTypes Reff2 on myBaseTable.dataTypeId=Reff2.id left join FeplFields Reff3 on myBaseTable.foreignKeyId=Reff3.id left join FeplTables Reff4 on myBaseTable.foreignTableId=Reff4.id  WHERE myBaseTable.id = @id";
                    return db.Query<FeplField, FeplTable, FeplDataType, FeplField, FeplTable, FeplField>(query,
                    (myBaseTable, ref1, ref2, ref3, ref4) =>
                    {
                        myBaseTable.tableNav = ref1;
                        myBaseTable.dataTypeNav = ref2;
                        myBaseTable.foreignKeyNav = ref3;
                        myBaseTable.foreignTableNav = ref4;
                        return myBaseTable;
                    }, new { id = id }).FirstOrDefault() ?? new FeplField();
                }
                else if (fillStyle == Utility.FillStyle.WithFullNav)
                {
                    query = "SELECT myBaseTable.*,Reff1.*,Reff2.*,Reff3.*,Reff4.* FROM FeplFields myBaseTable left join FeplTables Reff1 on myBaseTable.tableId=Reff1.id left join FeplDataTypes Reff2 on myBaseTable.dataTypeId=Reff2.id left join FeplFields Reff3 on myBaseTable.foreignKeyId=Reff3.id left join FeplTables Reff4 on myBaseTable.foreignTableId=Reff4.id  WHERE myBaseTable.id = @id";
                    return db.Query<FeplField, FeplTable, FeplDataType, FeplField, FeplTable, FeplField>(query,
                    (myBaseTable, ref1, ref2, ref3, ref4) =>
                    {
                        myBaseTable.tableNav = ref1;
                        myBaseTable.dataTypeNav = ref2;
                        myBaseTable.foreignKeyNav = ref3;
                        myBaseTable.foreignTableNav = ref4;
                        return myBaseTable;
                    }, new { id = id }).FirstOrDefault() ?? new FeplField();
                }
                else
                {
                    query = "Select myBaseTable.* from FeplFields myBaseTable where myBaseTable.tableId = '"+id+"'";
                    return db.Query<FeplField>(query).FirstOrDefault() ?? new FeplField();
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
                        numId = db.ExecuteScalar<int>("select Max(numId) from FeplFields where idPrefix = 'F'") + 1;
                        id = idPrefix + numId;
                        string sql = "INSERT INTO FeplFields (id, tableId, name, dataTypeId, length, isPrimaryKey, isRequired, numId, idPrefix, isHidden,metaRequired, foreignKeyId, printName, defaultValue, foreignTableId,metaTableId) VALUES (@id, @tableId, @name, @dataTypeId, @length, @isPrimaryKey, @isRequired, @numId, @idPrefix, @isHidden,@metaRequired, @foreignKeyId, @printName, @defaultValue, @foreignTableId,@metaTableId)";
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
        public bool Update()
        {
            using (IDbConnection db = new SQLiteConnection(Utility.ConnString))
            {
                db.Open();
                using (var transaction = db.BeginTransaction())
                {
                    try
                    {
                        string sql = "UPDATE FeplFields SET tableId = @tableId, name = @name, dataTypeId = @dataTypeId, length = @length, isPrimaryKey = @isPrimaryKey, isRequired = @isRequired, numId = @numId, idPrefix = @idPrefix, isHidden = @isHidden,metaRequired=@metaRequired, foreignKeyId = @foreignKeyId, printName = @printName, defaultValue = @defaultValue, foreignTableId = @foreignTableId ,metaTableId=@metaTableId WHERE id = @id;";
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
                        string sql = "DELETE FROM FeplFields WHERE id = @id;";
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
