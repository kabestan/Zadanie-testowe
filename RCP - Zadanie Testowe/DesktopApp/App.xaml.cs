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
            ImportFile();
        }

        private void ImportFile()
        {
            OpenFileDialog selectFile = new OpenFileDialog
            {
                DefaultExt = ".txt",
                Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*"
            };

            if (selectFile.ShowDialog() != true) { Shutdown(); return; }

            var file = new StreamReader(selectFile.OpenFile());

            uint recordsParsed = 0;
            uint recordsImported = 0;
            while (true)
            {
                string line = file.ReadLine();
                if (line == null) break;
                if (ValidateLogLine(line) == false) continue;
                //parse
                recordsParsed++;
                //insert
                recordsImported++;
            }

            new ButtonDialog(String.Format("Parsed and imported {0} records.", recordsParsed), "Ok").ShowDialog();

            Shutdown();
        }

        private bool ValidateLogLine(string line)
        {
            return true;
        }

        private void ParseLogLine(string line) 
        {

        }

        static private void log(object o)
        {
            System.Diagnostics.Debug.WriteLine(o);
        }
    }
}
