using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace CommonCode
{
    public class DatabaseOperator
    {
        private const string dbName = "RCPdb";
        private string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        /// <summary>
        /// Create object which controls connection to database.
        /// </summary>
        public DatabaseOperator()
        {
            Task dupsk(SqlCommand command)
            {
                if (DatabaseExists(command) == false)
                {
                    CreateDatabaseOnServer(command);
                    command.Connection.ChangeDatabase(dbName);
                    CreateTable(command);
                }
                connectionString += ";Initial Catalog=" + dbName;
                return Task.CompletedTask;
            }
            Connect(dupsk).Wait();
        }

        /// <summary>
        /// Converts list of records to one string query and sends to server.
        /// </summary>
        /// <param name="records">List of records to send.</param>
        /// <returns>Number of affected rows.</returns>
        public async Task<int> UploadRecords(List<Record> records)
        {
            string query = records.Select(ConvertRecordToInsert).Aggregate((a, b) => a + b);
            int affectedRows = 0;
            await Connect(async (command) =>
            {
                command.CommandText = query;
                affectedRows = await command.ExecuteNonQueryAsync();
            });
            return affectedRows;
        }

        private async Task Connect(Func<SqlCommand, Task> callback)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("", connection))
                {
                    await callback(command);
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
            command.CommandText =
@"CREATE TABLE [RCPlogs]
(
	[RecordId] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	[Timestamp] SMALLDATETIME NOT NULL,
    [WorkerId] INT NOT NULL, 
    [ActionType] TINYINT NOT NULL, 
    [LoggerType] TINYINT NOT NULL
)";
            command.ExecuteNonQuery();
        }

        private string ConvertRecordToInsert(Record record)
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

    public class Record
    {
        public int? RecordId { get; set; }
        public DateTime Timestamp { get; set; }
        public int WorkerId { get; set; }
        public Activity ActionType { get; set; }
        public Logger LoggerType { get; set; }

        public enum Activity
        {
            Entry = 0,
            Exit = 1,
            Service = 5
        }

        public enum Logger
        {
            Fingerprint = 0,
            Keypad = 1
            // z wartościami dotyczącymi tego był prawdopodobnie błąd w zadaniu
        }

        private enum Column
        {
            Year = 0,
            Month,
            Day,
            Hour,
            Minute,
            Worker,
            Action,
            Logger
        }

        private Record() { }

        public static Record CreateFromString(string rawTextLine)
        {
            try
            {
                Record record = new Record();
                int[] fields = rawTextLine.Split("-;".ToCharArray()).Select(int.Parse).ToArray();
                record.Timestamp = new DateTime(
                    fields[(int)Column.Year],
                    fields[(int)Column.Month],
                    fields[(int)Column.Day],
                    fields[(int)Column.Hour],
                    fields[(int)Column.Minute],
                    0);
                record.WorkerId = fields[(int)Column.Worker];
                record.ActionType = (Activity)fields[(int)Column.Action];
                record.LoggerType = (Logger)fields[(int)Column.Logger];
                return record;
            }
            catch (Exception e)
            {
                if (e is IndexOutOfRangeException || e is FormatException || e is NullReferenceException) { return null; }
                throw;
            }
        }

        public static Record CreateFromReader(IDataRecord reader)
        {
            throw new NotImplementedException();
        }
    }
}
