using System;
using System.IO;
using ADOX;
using System.Collections;
using System.Timers;

namespace TelegramBot
{
    class DBHelper
    {
        static DBHelper dbh;
        readonly string dbPath = "database\\db.mdb";
        readonly string connectionString;
        ADODB.Connection conn;
        public static object lockObj = new object();

        DBHelper()
        {
            connectionString = $"Provider=Microsoft.Jet.OLEDB.4.0; Data Source={dbPath}; Jet OLEDB:Engine Type=5";

            if (!File.Exists(dbPath))
            {
                CreateNewAccessDatabase();
            }

            conn = new ADODB.Connection();
        }

        public static DBHelper Init()
        {
            if (dbh == null)
            {
                dbh = new DBHelper();
            }
            return dbh;
        }

        public void Insert(string query)
        {
            try
            {
                System.Threading.Monitor.Enter(lockObj);
                object ret;
                conn.Open(connectionString, "", "", -1);
                conn.Execute(query, out ret, 0);
            }
            catch (Exception ex)
            {
                string error = "Ошибка при инсерте в БД";
                Logger.Write($"\n{error}:\n{new string('-', 10)}\n{ex.Message}\n{new string('-', 10)}");
            }
            finally
            {
                conn.Close();
                System.Threading.Monitor.Exit(lockObj);
            }
        }

        bool CreateNewAccessDatabase()
        {
            bool result = false;

            var cat = new Catalog();
            var table = new Table();


            try
            {
                if (!Directory.Exists("database"))
                {
                    Directory.CreateDirectory("database");
                }

                //Create the table and it's fields. 
                table.Name = "Items";

                cat.Create(connectionString);
                cat.Tables.Append(table);
                
                //table.Columns.Append("id", ADOX.DataTypeEnum.adInteger);
                table.Columns.Append("market_id", DataTypeEnum.adInteger);
                table.Columns.Append("dt", DataTypeEnum.adDate);
                table.Columns.Append("price", DataTypeEnum.adDouble);
                //table.Columns["price"].Properties["Nullable"].Value = false;
                var col = new Column();
                col.Name = "id";
                col.Type = DataTypeEnum.adInteger;
                col.ParentCatalog = cat;
                col.Properties["AutoIncrement"].Value = true;
                table.Columns.Append(col);
                table.Keys.Append("pk", KeyTypeEnum.adKeyPrimary, "id");

                //Now Close the database
                ADODB.Connection con = cat.ActiveConnection as ADODB.Connection;
                if (con != null)
                    con.Close();

                result = true;
            }
            catch (Exception ex)
            {
                result = false;
            }
            cat = null;
            return result;
        }
    }
}
