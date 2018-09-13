using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace BulkProcessing.DB
{
    public class BulkProcessDAO
    {
        Database db = new Database();

        // run only at first use
        public void InitializeDB()
        {
            db.InitializeDB();
        }

        public Dictionary<string, DateTime> GetDirectoryInfo()
        {
            Dictionary<string, DateTime> infoDictionary = new Dictionary<string, DateTime>();
            SQLiteConnection connection = db.OpenConnection();

            string sql = "SELECT * FROM DictionaryInfo;";
            SQLiteCommand command = new SQLiteCommand(sql, connection);
            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                infoDictionary.Add(
                    reader["Name"].ToString(),
                    DateTime.Parse(reader["ModifiedDate"].ToString()));
            }
            db.CloseConnection();

            return infoDictionary;
        }

        public void InsertNewDirectories(Dictionary<string, DateTime> newDirectories, List<string> responseUrls)
        {
            int index = 0;
            SQLiteConnection connection = db.OpenConnection();
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "INSERT INTO DictionaryInfo (Name, ModifiedDate, XmlResponse) values (@Name, @ModifiedDate, @XmlResponse)";

            foreach (KeyValuePair<string, DateTime> newDirectory in newDirectories)
            {
                command.Parameters.Add(new SQLiteParameter("@Name", newDirectory.Key));
                command.Parameters.Add(new SQLiteParameter("@ModifiedDate", newDirectory.Value));
                command.Parameters.Add(new SQLiteParameter("@XmlResponse", responseUrls[index++]));
                command.ExecuteNonQuery();
            }

            db.CloseConnection();
        }
    }
}
