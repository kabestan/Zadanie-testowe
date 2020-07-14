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
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            new ButtonDialog("Hello there. Select file to import...", "Select").ShowDialog();

            DatabaseOperator db = new DatabaseOperator();

            ImportFile(db);
        }

        private void ImportFile(DatabaseOperator db)
        {
            OpenFileDialog selectFile = new OpenFileDialog
            {
                DefaultExt = ".txt",
                Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*"
            };

            if (selectFile.ShowDialog() != true) { Shutdown(); return; }

            var file = new StreamReader(selectFile.OpenFile());

            uint linesInFile = 0;
            uint recordsParsed = 0;
            uint recordsImported = 0;
            while (true)
            {
                string line = file.ReadLine();
                if (line == null) break;
                linesInFile++;

                Record record = Record.CreateFromString(line);
                if (record == null) continue;
                recordsParsed++;

                if (db.UploadRecord(record) == false) continue;
                recordsImported++;
            }

            new ButtonDialog(String.Format("Of {0} lines in file, {1} was parsed and {2} imported.", linesInFile, recordsParsed, recordsImported), "Ok").ShowDialog();

            Shutdown();
        }

        static private void log(object o)
        {
            System.Diagnostics.Debug.WriteLine(o);
        }
    }
}
