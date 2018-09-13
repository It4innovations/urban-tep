using System.Data.SQLite;
using System.IO;

namespace BulkProcessing.DB
{
    public class Database
    {
        private static string DATABASE_NAME = "BulkProcessingDB.sqlite";
        private SQLiteConnection DbConnection;

        private static readonly log4net.ILog Log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public SQLiteConnection OpenConnection()
        {
            DbConnection = new SQLiteConnection("Data Source=" + DATABASE_NAME + ";Version=3;");
            DbConnection.Open();

            return DbConnection;
        }

        public void CloseConnection()
        {
            DbConnection.Close();
        }

        public void InitializeDB()
        {
            string dirPath = Directory.GetCurrentDirectory();

            if (!File.Exists(Path.Combine(dirPath, DATABASE_NAME)))
            {
                Log.Debug("Database is not created yet. Creating new DB file and table...");
                CreateDatabaseFile();
                CreateTable();
            }
        }

        private void CreateDatabaseFile()
        {
            SQLiteConnection.CreateFile(DATABASE_NAME);
        }

        private void CreateTable()
        {
            OpenConnection();
            string sql = "CREATE TABLE DictionaryInfo (Name VarChar(100) NOT NULL, ModifiedDate DateTime NOT NULL, XmlResponse VarChar(255) NULL);";
            SQLiteCommand command = new SQLiteCommand(sql, DbConnection);
            command.ExecuteNonQuery();
            CloseConnection();
        }
    }
}
