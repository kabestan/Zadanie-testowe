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
        public const int defaultRecordsIncrement = 100;

        private const string dbName = "RCPdb";
        private static string connectionString = null;
        private static bool DatabaseNotInitialized { get { return connectionString == null; } }

        /// <summary>
        /// Converts list of records to one string query and sends to server.
        /// </summary>
        /// <param name="records">List of records to send.</param>
        /// <returns>Number of affected rows.</returns>
        public static async Task<int> UploadRecords(List<Record> records)
        {
            string query = records.Select(ConvertRecordToInsertQuery).Aggregate((a, b) => a + b);
            return await Connect(async (command) =>
            {
                command.CommandText = query;
                return await command.ExecuteNonQueryAsync();
            });
        }

        /// <summary>
        /// Retrieves records from database, starting with id, ordered by id and up to amount, specified by parameters.
        /// </summary>
        /// <param name="startingId">Starting id of records group. Select last records if null.</param>
        /// <param name="amount">Amount of records to retrieve.</param>
        /// <returns><see cref="DataTable"/> containing selected records.</returns>
        public static async Task<DataTable> DownloadRecords(int? startingId = null, int amount = defaultRecordsIncrement)
        {
            return await WrappedRecordsReader(startingId, amount, (reader) =>
            {
                var table = new DataTable();
                table.Columns.Add(new DataColumn("RecordId", typeof(int)));
                table.Columns.Add(new DataColumn("Timestamp", typeof(DateTime)));
                table.Columns.Add(new DataColumn("WorkerId", typeof(int)));
                table.Columns.Add(new DataColumn("ActionType", typeof(Record.Activity)));
                table.Columns.Add(new DataColumn("LoggerType", typeof(Record.Logger)));
                table.Load(reader);
                return Task.FromResult(table);
            });
        }

        /// <summary>
        /// Retrieves records from database, starting with id, ordered by id and up to amount, specified by parameters.
        /// </summary>
        /// <param name="startingId">Starting id of records group. Select last records if null.</param>
        /// <param name="amount">Amount of records to retrieve.</param>
        /// <returns><see cref="List{T}"/> containing selected records.</returns>
        public static async Task<List<Record>> DownloadRecordsAsList(int? startingId = null, int howMany = defaultRecordsIncrement)
        {
            return await WrappedRecordsReader(startingId, howMany, async (reader) =>
            {
                var records = new List<Record>();
                while (await reader.ReadAsync())
                {
                    records.Add(Record.CreateFromReader(reader));
                }
                return records;
            });
        }

        /// <summary>
        /// Executes query on SQL server to create report according to assignment.
        /// </summary>
        /// <returns><see cref="DataTable"/> with created report.</returns>
        public static async Task<DataTable> CreateReport()
        {
            return await Connect(async (command) =>
            {
                command.CommandText = Properties.Resources.GetReport;
                var report = new DataTable();
                report.Load(await command.ExecuteReaderAsync());
                return report;
            });
        }

        /// <summary>
        /// Ensure that database on server is ready to operate.
        /// </summary>
        /// <returns>Asynchronous task that will complete after database initialization.s</returns>
        private static async Task InitDatabase()
        {
            if (DatabaseNotInitialized)
            {
                connectionString = GetConnectionString(false);
                await Connect(async (command) =>
                {
                    if (await DatabaseExists(command) == false)
                    {
                    // Create database and table
                    await CreateDatabaseOnServer(command);
                        command.Connection.ChangeDatabase(dbName);
                        await CreateTable(command);

                    // Store inserting procedure
                    command.CommandText = Properties.Resources.InsertDistinctProcedure;
                        await command.ExecuteNonQueryAsync();
                    }
                    connectionString = GetConnectionString();
                    return Task.FromResult(true);
                });
            }
        }

        private static async Task<T> WrappedRecordsReader<T>(int? startingId, int howMany, Func<SqlDataReader, Task<T>> operateOnReader)
        {
            return await Connect(async (command) =>
            {
                command.CommandText = Properties.Resources.GetRecords;
                command.Parameters.Add("start", SqlDbType.Int).Value = startingId == null ? -1 : startingId;
                command.Parameters.Add("count", SqlDbType.Int).Value = howMany;

                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    return await operateOnReader(reader);
                }
            });
        }

        private static async Task<T> Connect<T>(Func<SqlCommand, Task<T>> operateOnCommand)
        {
            await InitDatabase();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand("", connection))
                {
                    var t = operateOnCommand(command);
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

        private static string GetConnectionString(bool withCatalog = true)
        {
            return @"Data Source=(localdb)\MSSQLLocalDB;" +
                "Integrated Security=True;" +
                "Connect Timeout=30;" +
                "Encrypt=False;" +
                "TrustServerCertificate=False;" +
                "ApplicationIntent=ReadWrite;" +
                "MultiSubnetFailover=False;" +
                (withCatalog ? $"Initial Catalog={dbName};" : "");
        }

        private static string ConvertRecordToInsertQuery(Record r)
        {
             return $"EXEC InsertDistinct @t = '{r.Timestamp}', @w = {r.WorkerId}, @a = {(int)r.ActionType}, @l = {(int)r.LoggerType};\n";
        }
    }
 }
