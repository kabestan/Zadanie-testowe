using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CommonCode
{
    public static class DatabaseOperator
    {
        public delegate Task<DataTable> RequestRecords(int? startingId, int count);

        private const string dbName = "RCPdb";
        private static string connectionString = null;

        /// <summary>
        /// Ensure that database on server is ready to operate.
        /// </summary>
        /// <returns>Asynchronous task that will complete after database initialization.s</returns>
        private static async Task InitDatabase()
        {
            if (connectionString != null) return;
            connectionString = GetConnectionString();
            await Connect(async (command) =>
            {
                if (await DatabaseExists(command) == false)
                {
                    await CreateDatabaseOnServer(command);
                    command.Connection.ChangeDatabase(dbName);
                    await CreateTable(command);
                }
                connectionString += ";Initial Catalog=" + dbName;
                return;
            });
        }

        /// <summary>
        /// Converts list of records to one string query and sends to server.
        /// </summary>
        /// <param name="records">List of records to send.</param>
        /// <returns>Number of affected rows.</returns>
        public static async Task<int> UploadRecords(List<Record> records)
        {
            string query = records.Select(ConvertRecordToInsertQuery).Aggregate((a, b) => a + b);
            int affectedRows = 0;
            await Connect(async (command) =>
            {
                command.CommandText = query;
                affectedRows = await command.ExecuteNonQueryAsync();
            });
            return affectedRows;
        }

        public static async Task<DataTable> CreateReport()
        {
            return await Connect(async (command) =>
            {
                var report = new DataTable();
                return report;
            });
        }

        public static async Task<DataTable> DownloadRecords(int? startingId = null, int howMany = 100)
        {
            return await Connect(async (command) =>
            {
                command.CommandText = "DECLARE @start int, @count int;\n";
                command.CommandText += $"SET @count = {howMany};\n";
                if (startingId == null)
                { command.CommandText += "SET @start = (SELECT MAX([RecordId]) - @count + 1 FROM[RCPlogs]);\n"; }
                else
                { command.CommandText += $"SET @start = {startingId};\n"; }
                command.CommandText += "SELECT * FROM [RCPlogs] WHERE [RecordId] >= @start AND [RecordId] < @start + @count;";
                
                var table = new DataTable();
                table.Columns.Add(new DataColumn("RecordId", typeof(int)));
                table.Columns.Add(new DataColumn("Timestamp", typeof(DateTime)));
                table.Columns.Add(new DataColumn("WorkerId", typeof(int)));
                table.Columns.Add(new DataColumn("ActionType", typeof(Record.Activity)));
                table.Columns.Add(new DataColumn("LoggerType", typeof(Record.Logger)));
                table.Load(await command.ExecuteReaderAsync());
                return table;
            });
        }

        private static async Task Connect(Func<SqlCommand, Task> callback)
        {
            await Connect<object>(async (command) =>
            {
                await callback(command);
                return default;
            });
        }

        private static async Task<T> Connect<T>(Func<SqlCommand, Task<T>> callback)
        {
            await InitDatabase();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand("", connection))
                {
                    var t = callback(command);
                    Debug.WriteLine($"--- SqlConnection > SqlCommand > CommandText:\n{command.CommandText}");
                    return await t;
                }
            }
        }

        private static async Task<bool> DatabaseExists(SqlCommand command)
        {
            command.CommandText = @"
DECLARE @dbname nvarchar(128)
SET @dbname = N'" + dbName + @"'

SELECT name 
FROM master.dbo.sysdatabases 
WHERE ('[' + name + ']' = @dbname OR name = @dbname)";

            using (SqlDataReader reader = await command.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    if (reader.GetString(0) == dbName) return true;
                }
                return false;
            }
        }

        private static async Task CreateDatabaseOnServer(SqlCommand command)
        {
            command.CommandText = $"CREATE DATABASE {dbName};";
            await command.ExecuteNonQueryAsync();
        }

        private static async Task CreateTable(SqlCommand command)
        {
            command.CommandText =
@"CREATE TABLE [RCPlogs]
(
	[RecordId] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	[Timestamp] SMALLDATETIME NOT NULL,
    [WorkerId] INT NOT NULL, 
    [ActionType] TINYINT NOT NULL, 
    [LoggerType] TINYINT NOT NULL
)";
            await command.ExecuteNonQueryAsync();
        }

        private static string GetConnectionString()
        {
            return "Data Source=(localdb)\\MSSQLLocalDB;" +
                "Integrated Security=True;" +
                "Connect Timeout=30;" +
                "Encrypt=False;" +
                "TrustServerCertificate=False;" +
                "ApplicationIntent=ReadWrite;" +
                "MultiSubnetFailover=False;";
        }

        private static string ConvertRecordToInsertQuery(Record record)
        {
            return String.Format(
                @"INSERT INTO [dbo].[RCPlogs] ([Timestamp], [WorkerId], [ActionType], [LoggerType]) VALUES ('{0}', {1}, {2}, {3}) ",
                record.Timestamp.ToString(),
                record.WorkerId.ToString(),
                ((int)record.ActionType).ToString(),
                ((int)record.LoggerType).ToString()
            );
        }
    }
 }
