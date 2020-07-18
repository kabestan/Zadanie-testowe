using System;
using System.Data;
using System.Linq;

namespace CommonCode
{
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
