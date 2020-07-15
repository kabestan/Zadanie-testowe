using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CommonCode;
using System.Data.SqlClient;

namespace DesktopApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            DatabaseOperator db = new DatabaseOperator();
            MainWindow mainWindow = new MainWindow();
            mainWindow.ImportButton.Click += (a, b) => { ImportFile(db); };
            mainWindow.ReportButton.Click += (a, b) => { Debug.WriteLine("Not implemented."); };
            mainWindow.Show();
        }

        private void ImportFile(DatabaseOperator db)
        {
            OpenFileDialog selectFile = new OpenFileDialog
            {
                DefaultExt = ".txt",
                Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*"
            };

            if (selectFile.ShowDialog() != true) { return; }

            int linesInFile = CountLines(selectFile.OpenFile()); ;
            int recordsParsed = 0;
            int recordsImported = 0;
            int limitRecordsBatchList = 200;

            using (StreamReader reader = new StreamReader(selectFile.OpenFile()))
                while (true)
                {
                    List<Record> records = ReadRecordsToList(limitRecordsBatchList, reader);
                    if (records.Count <= 0) break;
                    recordsParsed += records.Count;
                    recordsImported += db.UploadRecords(records);
                    Debug.WriteLine(recordsImported);
                    if (records.Count < limitRecordsBatchList) break;
                }

            new ButtonDialog(String.Format("Of {0} lines in file, {1} was parsed and {2} imported.", linesInFile, recordsParsed, recordsImported), "Ok").ShowDialog();
        }

        private List<Record> ReadRecordsToList(int count, StreamReader reader)
        {
            List<Record> list = new List<Record>();
            for (int i = 0; i < count; i++)
            {
                string line = reader.ReadLine();
                if (line == null) break;

                Record record = Record.CreateFromString(line);
                if (record == null) continue;

                list.Add(record);
            }
            return list;
        }

        static private int CountLines(Stream stream)
        {
            // performance: 260ms for 650k lines
            int count = 0;
            using (StreamReader reader = new StreamReader(stream))
                while (reader.ReadLine() != null) 
                    count++;
            return count;
        }
    }
}
