using CommonCode;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace DesktopApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private MainWindow mainWindow;

        /// <summary>
        /// Application starting point.
        /// </summary>
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                Trace.TraceInformation("Application Startup.");
                mainWindow = new MainWindow();
                mainWindow.ImportButton.Click += OnImportButtonClick;
                mainWindow.BrowseButton.Click += OnBrowseButtonClick;
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
                ButtonDialog.ShowProblem();
            }
        }

        private async void OnImportButtonClick(object s, RoutedEventArgs e)
        {
            try
            {
                mainWindow.ImportButton.IsEnabled = false;
                mainWindow.BrowseButton.IsEnabled = false;
                await ImportFile((progress) =>
                {
                    mainWindow.TextField.Text = $"Importing... {(progress * 100):0.0}%";
                });
                OnBrowseButtonClick(s, e);
                mainWindow.ImportButton.IsEnabled = true;
                mainWindow.BrowseButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
                ButtonDialog.ShowProblem();
            }
        }

        private void OnBrowseButtonClick(object s, RoutedEventArgs e)
        {
            try
            {
                mainWindow.Hide();
                DataViewWindow dataView = new DataViewWindow(DatabaseOperator.DownloadRecords);
                dataView.Closed += (a, b) => { mainWindow.Show(); };
                dataView.TheReportButton.Click += OnReportButtonClick;
                dataView.Show();
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
                ButtonDialog.ShowProblem();
            }
        }

        private async void OnReportButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Button reportButton = sender as Button;
                reportButton.IsEnabled = false;
                ReportWindow reportWindow = new ReportWindow(await DatabaseOperator.CreateReport());
                reportWindow.Closed += (s, f) => { reportButton.IsEnabled = true; };
                reportWindow.Show();
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
                ButtonDialog.ShowProblem();
            }
        }

        private async Task ImportFile(Action<float> ProgressUpdate)
        {
            Trace.TraceInformation("File import started.");
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
            Stopwatch totalTime = new Stopwatch();
            totalTime.Start();
            ProgressUpdate(0);

            using (StreamReader reader = new StreamReader(selectFile.OpenFile()))
                while (true)
                {
                    List<Record> records = ReadRecordsToList(limitRecordsBatchList, reader);
                    if (records.Count <= 0) break;
                    recordsParsed += records.Count;
                    recordsImported += await DatabaseOperator.UploadRecords(records);
                    ProgressUpdate(recordsParsed / (float)linesInFile);
                    if (records.Count < limitRecordsBatchList) break;
                }

            string txt = $"Of {linesInFile} lines in file, {recordsParsed} was parsed and {recordsImported} imported.";
            txt += $" Elapsed {(totalTime.ElapsedMilliseconds / 1000f):0.00}s.";
            ButtonDialog.Show(txt, "Ok");
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
