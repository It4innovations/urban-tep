using System;
using System.Data;
using System.Data.SqlClient;

namespace UserProcessorDB.DAO
{
    public class NamespaceInfoDAO
    {
        private static String MsSqlConnectionString = Properties.Settings.Default.MsSqlConnectionString;

        public long CreateNamespaceInfo()
        {
            long id = -1;

            using (IDbConnection connection = new SqlConnection(MsSqlConnectionString))
            {
                connection.Open();
                IDbCommand command = connection.CreateCommand();
                command.CommandText =
                    @"INSERT INTO NamespaceInfo DEFAULT VALUES; 
                    SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";

                id = (long)command.ExecuteScalar();
                connection.Close();
            }

            return id;
        }
    }
}
