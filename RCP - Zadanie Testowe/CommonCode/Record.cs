using System;
using System.ComponentModel;
using System.Data;
using System.Linq;

// TODO: possible refactor: implement cast operators for Record class
// TODO: change Record.RecordId type int? to int and correct all dependent code

namespace CommonCode
{
    public class Record
    {

        [DisplayName("Id")] public int? RecordId { get; set; }
        [DisplayName("Date")] public DateTime Timestamp { get; set; }
        [DisplayName("Employee")] public int WorkerId { get; set; }
        [DisplayName("Type")] public Activity ActionType { get; set; }
        [DisplayName("Logger")] public Logger LoggerType { get; set; }

        public enum Activity
        {
            Entry = 0,
            Exit = 1,
            Service = 5,
            unnamed_3 = 3,
            unnamed_4 = 4,
            unnamed_7 = 7
        }

        public enum Logger
        {
            Fingerprint = 0,
            Keypad = 1
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
            Func<string, object> getField = (name) =>
            {
                return reader[reader.GetOrdinal(name)];
            };

            Record record = new Record();

            record.RecordId = Convert.ToInt32(getField("RecordId"));
            record.Timestamp = Convert.ToDateTime(getField("Timestamp"));
            record.WorkerId = Convert.ToInt32(getField("WorkerId"));
            record.ActionType = (Activity)Convert.ToInt32(getField("ActionType"));
            record.LoggerType = (Logger)Convert.ToInt32(getField("LoggerType"));

            return record;
        }
    }
}
