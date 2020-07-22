using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DesktopApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ButtonDialog : Window
    {
        private ButtonDialog()
        {
            InitializeComponent();
        }

        public static ButtonDialog Show(string textMessage, string buttonLabel)
        {
            ButtonDialog dialog = new ButtonDialog();
            dialog.TheText.Text = textMessage;
            dialog.TheButton.Content = buttonLabel;
            return dialog;
        }

        public static ButtonDialog ShowProblem()
        {
            return Show("There was some trouble. For more info check trace.log file.", "OK");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
