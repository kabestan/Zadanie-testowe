using System;
using System.Collections.Generic;
using System.Data;
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
using System.Windows.Shapes;

namespace DesktopApp
{
    /// <summary>
    /// Interaction logic for ReportWindow.xaml
    /// </summary>
    public partial class ReportWindow : Window
    {
        public ReportWindow(DataTable report)
        {
            InitializeComponent();
            TheGrid.ItemsSource = report.DefaultView;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(TheGrid.ItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("YearMonth");
            view.GroupDescriptions.Add(groupDescription);

        }
    }
}
