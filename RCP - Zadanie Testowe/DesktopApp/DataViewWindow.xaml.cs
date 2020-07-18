using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
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

// TODO: dynamically resize dataGrid columns accordingly;
// TODO: navigate to record;
// TODO: navigating arrows;

namespace DesktopApp
{
    /// <summary>
    /// Interaction logic for DataViewWindow.xaml
    /// </summary>
    public partial class DataViewWindow : Window
    {
        private readonly Func<int, int?, Task<DataTable>> RequestData;
        private const int startingIdIncrement = 100;
        private int? currentStartingId;

        public DataViewWindow(Func<int, int?, Task<DataTable>> requestDataDelegate)
        {
            InitializeComponent();

            RequestData = requestDataDelegate;
            BindNewDataToGrid();
        }

        private async void BindNewDataToGrid(int count = startingIdIncrement, int? startingId = null)
        {
            LoadingMode(true);
            {
                DataTable dataTable = await RequestData(count, startingId);
                TheGrid.ItemsSource = dataTable.DefaultView;
                if (dataTable.Rows.Count > 0) 
                { 
                    currentStartingId = dataTable.Rows[0].Field<int>("RecordId");
                    TheSelectIdTextBox.Text = currentStartingId.ToString();
                }
                else 
                { 
                    currentStartingId = null;
                    TheSelectIdTextBox.Text = "";
                }
            }
            LoadingMode(false);
        }

        private void LoadingMode(bool setToOn)
        {
            TheButtonsPanel.IsEnabled = !setToOn;
            TheLoadingPanel.Visibility = setToOn ? Visibility.Visible : Visibility.Hidden;
        }
    }
}
