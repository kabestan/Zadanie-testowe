using CommonCode;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace DesktopApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private MainWindow mainWindow;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            mainWindow = new MainWindow();
            mainWindow.ImportButton.Click += OnImportButtonClick;
            mainWindow.BrowseButton.Click += OnBrowseButtonClick;
            mainWindow.Show();
        }
        
        private async void OnImportButtonClick(object s, RoutedEventArgs e)
        {
            mainWindow.ImportButton.IsEnabled = false;
            mainWindow.BrowseButton.IsEnabled = false;
            await ImportFile((progress) =>
            {
                mainWindow.TextField.Text = $"Importing... {(progress * 100):0.0}%";
            });
            mainWindow.ImportButton.IsEnabled = true;
            mainWindow.BrowseButton.IsEnabled = true;
        }

        private void OnBrowseButtonClick(object s, RoutedEventArgs e)
        {
            mainWindow.Hide();
            DataViewWindow dataView = new DataViewWindow(DatabaseOperator.DownloadRecords);
            dataView.Show();
            dataView.Closed += (a, b) => { mainWindow.Show(); };
        }

        private async Task ImportFile(Action<float> ProgressUpdate)
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
            Stopwatch middleTime = new Stopwatch();
            Stopwatch totalTime = new Stopwatch();
            totalTime.Start();

            using (StreamReader reader = new StreamReader(selectFile.OpenFile()))
                while (true)
                {
                    middleTime.Restart();
                    List<Record> records = ReadRecordsToList(limitRecordsBatchList, reader);
                    if (records.Count <= 0) break;
                    recordsParsed += records.Count;
                    ProgressUpdate(recordsParsed / (float)linesInFile);
                    recordsImported += await DatabaseOperator.UploadRecords(records);
                    Debug.WriteLine(recordsImported + " - " + middleTime.ElapsedMilliseconds / 1000f);
                    if (records.Count < limitRecordsBatchList) break;
                }

            string txt = $"Of {linesInFile} lines in file, {recordsParsed} was parsed and {recordsImported} imported.";
            txt += $" Elapsed {(totalTime.ElapsedMilliseconds / 1000f):0.00}s.";
            new ButtonDialog(txt, "Ok").ShowDialog();
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
