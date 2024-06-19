using System.Data.SQLite;
using System.Data;
using Dapper;


namespace Scaffolding.Models
{
    public class FeplDataType
    {
        public string id { get; set; } = "";
        public string name { get; set; } = "";
        public string cSharp { get; set; } = "";
        public string sqLite { get; set; } = "";
        public int numId { get; set; } = 0;
        public string idPrefix { get; set; } = "";
        // Additional methods
        public static List<FeplDataType> Get(Utility.FillStyle fillStyle = Utility.FillStyle.AllProperties, Dictionary<string, string>? paramList = null)
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
                    query = "SELECT id,name from FeplDataTypes";
                    return db.Query<FeplDataType>(query).ToList();
                }
                else if (fillStyle == Utility.FillStyle.AllProperties)
                {
                    query = "SELECT * from FeplDataTypes";
                    return db.Query<FeplDataType>(query).ToList();
                }
                else
                {
                    query = "";
                    return db.Query<FeplDataType>(query).ToList();
                }
            }
        }
        public static FeplDataType GetById(string id, Utility.FillStyle fillStyle = Utility.FillStyle.Basic)
        {
            using (IDbConnection db = new SQLiteConnection(Utility.ConnString))
            {
                db.Open();
                string query = "";
                if (fillStyle == Utility.FillStyle.Basic)
                {
                    query = "SELECT id,name from FeplDataTypes WHERE id = @id";
                    return db.Query<FeplDataType>(query, new { id = id }).FirstOrDefault() ?? new FeplDataType();
                }
                else if (fillStyle == Utility.FillStyle.AllProperties)
                {
                    query = "SELECT * from FeplDataTypes WHERE id = @id";
                    return db.Query<FeplDataType>(query, new { id = id }).FirstOrDefault() ?? new FeplDataType();
                }
                else
                {
                    query = "";
                    return db.Query<FeplDataType>(query).FirstOrDefault() ?? new FeplDataType();
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
                        numId = db.ExecuteScalar<int>("select Max(numId) from FeplDataTypes where idPrefix = 'F'") + 1;
                        id = idPrefix + numId;
                        string sql = "INSERT INTO FeplDataTypes (id, name, cSharp, sqLite, numId, idPrefix) VALUES (@id, @name, @cSharp, @sqLite, @numId, @idPrefix)";
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
                        string sql = "UPDATE FeplDataTypes SET name = @name, cSharp = @cSharp, sqLite = @sqLite, numId = @numId, idPrefix = @idPrefix WHERE id = @id;";
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
                        string sql = "DELETE FROM FeplDataTypes WHERE id = @id;";
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