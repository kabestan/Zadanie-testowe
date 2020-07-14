using System;
using System.Data.SqlClient;
using System.Diagnostics;


namespace CommonCode
{
    public class DatabaseOperator
    {
        private const string dbName = "RCPdb";
        private string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        public DatabaseOperator()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("", connection))
                {
                    if (DatabaseExists(command) == false)
                    {
                        CreateDatabaseOnServer(command);
                        connectionString += ";Initial Catalog=" + dbName;
                        connection.ChangeDatabase(dbName);
                        CreateTable(command);
                    }
                }
            }
        }

        private bool DatabaseExists(SqlCommand command)
        {
            command.CommandText = @"
DECLARE @dbname nvarchar(128)
SET @dbname = N'" + dbName + @"'

SELECT name 
FROM master.dbo.sysdatabases 
WHERE ('[' + name + ']' = @dbname OR name = @dbname)";

            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (reader.GetString(0) == dbName) return true;
                }
                return false;
            }
        }

        private void CreateDatabaseOnServer(SqlCommand command)
        {
            command.CommandText = "CREATE DATABASE " + dbName;
            command.ExecuteNonQuery();
        }

        private void CreateTable(SqlCommand command)
        {
            command.CommandText = @"
CREATE TABLE [RCPlogs]
(
	[RecordId] INT NOT NULL PRIMARY KEY,
	[Timestamp] SMALLDATETIME NOT NULL, 
    [WorkerId] INT NOT NULL, 
    [ActionType] TINYINT NOT NULL, 
    [LoggerType] TINYINT NOT NULL
)";
            command.ExecuteNonQuery();
        }
    }
}
