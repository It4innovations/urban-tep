using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using UserProcessorDB.Data;

namespace UserProcessorDB.DAO
{
    public class UserProcessorDAO
    {
        private static String MsSqlConnectionString = Properties.Settings.Default.MsSqlConnectionString;

        public UserProcessorDTO ConvertToDTO(IDataReader reader)
        {
            UserProcessorDTO userProcessor = new UserProcessorDTO();
            userProcessor.FullProcessorName = reader["FullProcessorName"].ToString();
            userProcessor.NamespaceInfoId = long.Parse(reader["NamespaceInfoId"].ToString());
            userProcessor.TemplateId = long.Parse(reader["TemplateId"].ToString());
            userProcessor.GeoserverName = reader["GeoserverName"].ToString();
            userProcessor.UserName = reader["UserName"].ToString();
            userProcessor.ProcessorName = reader["ProcessorName"].ToString();
            userProcessor.Version = reader["Version"].ToString();
            userProcessor.CreationalTime = DateTime.Parse(reader["CreationalTime"].ToString());
            userProcessor.IsActive = bool.Parse(reader["IsActive"].ToString());

            return userProcessor;
        }

        public UserProcessorDTO GetLastUserProcessor(string processorName)
        {
            UserProcessorDTO userProcessor = new UserProcessorDTO();

            using (IDbConnection connection = new SqlConnection(MsSqlConnectionString))
            {
                connection.Open();
                IDbCommand command = connection.CreateCommand();
                command.CommandText =
                    "SELECT TOP 1 * FROM UserProcessorInfo WHERE ProcessorName=@ProcessorName ORDER BY Version DESC;";

                command.Parameters.Add(new SqlParameter("@ProcessorName", SqlDbType.VarChar));
                ((SqlParameter)command.Parameters["@ProcessorName"]).Value = processorName;

                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        userProcessor = ConvertToDTO(reader);
                    }
                }
                connection.Close();
            }

            return userProcessor;
        }

        public List<UserProcessorDTO> GetAllUserProcessors(string userName)
        {
            List<UserProcessorDTO> userProcessors = new List<UserProcessorDTO>();

            using (IDbConnection connection = new SqlConnection(MsSqlConnectionString))
            {
                connection.Open();
                IDbCommand command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM UserProcessorInfo WHERE UserName=@UserName;";

                command.Parameters.Add(new SqlParameter("@UserName", SqlDbType.VarChar));
                ((SqlParameter)command.Parameters["@UserName"]).Value = userName;

                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        userProcessors.Add(
                            ConvertToDTO(reader));
                    }
                }
                connection.Close();
            }

            return userProcessors;
        }

        public void InsertNewUserProcessor(UserProcessorDTO userProcessor)
        {
            using (IDbConnection connection = new SqlConnection(MsSqlConnectionString))
            {
                connection.Open();
                IDbCommand command = connection.CreateCommand();
                command.CommandText =
                    @"INSERT INTO UserProcessorInfo(FullProcessorName, NamespaceInfoId, TemplateId, GeoserverName, UserName, ProcessorName, Version, CreationalTime, IsActive) 
                    VALUES (@FullProcessorName, @NamespaceInfoId, @TemplateId, @GeoserverName, @UserName, @ProcessorName, @Version, @CreationalTime, @IsActive)";

                command.Parameters.Add(new SqlParameter("@FullProcessorName", SqlDbType.VarChar));
                command.Parameters.Add(new SqlParameter("@NamespaceInfoId", SqlDbType.BigInt));
                command.Parameters.Add(new SqlParameter("@TemplateId", SqlDbType.BigInt));
                command.Parameters.Add(new SqlParameter("@GeoserverName", SqlDbType.VarChar));
                command.Parameters.Add(new SqlParameter("@UserName", SqlDbType.VarChar));
                command.Parameters.Add(new SqlParameter("@ProcessorName", SqlDbType.VarChar));
                command.Parameters.Add(new SqlParameter("@Version", SqlDbType.VarChar));
                command.Parameters.Add(new SqlParameter("@CreationalTime", SqlDbType.DateTime));
                command.Parameters.Add(new SqlParameter("@IsActive", SqlDbType.Bit));

                ((SqlParameter)command.Parameters["@FullProcessorName"]).Value = userProcessor.FullProcessorName;
                ((SqlParameter)command.Parameters["@NamespaceInfoId"]).Value = userProcessor.NamespaceInfoId;
                ((SqlParameter)command.Parameters["@TemplateId"]).Value = userProcessor.TemplateId;
                ((SqlParameter)command.Parameters["@GeoserverName"]).Value = userProcessor.GeoserverName;
                ((SqlParameter)command.Parameters["@UserName"]).Value = userProcessor.UserName;
                ((SqlParameter)command.Parameters["@ProcessorName"]).Value = userProcessor.ProcessorName;
                ((SqlParameter)command.Parameters["@Version"]).Value = userProcessor.Version;
                ((SqlParameter)command.Parameters["@CreationalTime"]).Value = userProcessor.CreationalTime;
                ((SqlParameter)command.Parameters["@IsActive"]).Value = userProcessor.IsActive;

                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public void UpdateUserProcessorActiveStatus(UserProcessorDTO userProcessor)
        {
            using (IDbConnection connection = new SqlConnection(MsSqlConnectionString))
            {
                connection.Open();
                IDbCommand command = connection.CreateCommand();
                command.CommandText =
                    "UPDATE UserProcessorInfo SET IsActive = @IsActive WHERE UserName = @UserName AND ProcessorName = @ProcessorName AND Version = @Version;";

                command.Parameters.Add(new SqlParameter("@UserName", SqlDbType.VarChar));
                command.Parameters.Add(new SqlParameter("@ProcessorName", SqlDbType.VarChar));
                command.Parameters.Add(new SqlParameter("@Version", SqlDbType.VarChar));
                command.Parameters.Add(new SqlParameter("@IsActive", SqlDbType.Bit));

                ((SqlParameter)command.Parameters["@UserName"]).Value = userProcessor.UserName;
                ((SqlParameter)command.Parameters["@ProcessorName"]).Value = userProcessor.ProcessorName;
                ((SqlParameter)command.Parameters["@Version"]).Value = userProcessor.Version;
                ((SqlParameter)command.Parameters["@IsActive"]).Value = userProcessor.IsActive;

                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public void DeleteUserProcessor(UserProcessorDTO userProcessor)
        {
            using (IDbConnection connection = new SqlConnection(MsSqlConnectionString))
            {
                connection.Open();
                IDbCommand command = connection.CreateCommand();
                command.CommandText = @"DELETE FROM UserProcessorInfo WHERE UserName = @UserName AND ProcessorName = @ProcessorName AND Version = @Version;
                DELETE FROM NamespaceInfo WHERE Id = @Id;";

                command.Parameters.Add(new SqlParameter("@UserName", SqlDbType.VarChar));
                command.Parameters.Add(new SqlParameter("@ProcessorName", SqlDbType.VarChar));
                command.Parameters.Add(new SqlParameter("@Version", SqlDbType.VarChar));
                command.Parameters.Add(new SqlParameter("@Id", SqlDbType.BigInt));

                ((SqlParameter)command.Parameters["@UserName"]).Value = userProcessor.UserName;
                ((SqlParameter)command.Parameters["@ProcessorName"]).Value = userProcessor.ProcessorName;
                ((SqlParameter)command.Parameters["@Version"]).Value = userProcessor.Version;
                ((SqlParameter)command.Parameters["@Id"]).Value = userProcessor.NamespaceInfoId;

                command.ExecuteNonQuery();
                connection.Close();
            }
        }
    }
}
