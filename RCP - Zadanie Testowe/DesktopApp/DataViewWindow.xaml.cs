using CommonCode;
using System;
using System.Data;
using System.Windows;
using System.Windows.Input;

namespace DesktopApp
{
    /// <summary>
    /// Interaction logic for DataViewWindow.xaml
    /// </summary>
    public partial class DataViewWindow : Window
    {
        private readonly DatabaseOperator.RequestRecords RequestData;
        private const int startingIdIncrement = 100;
        private int? currentStartingId;

        public DataViewWindow(DatabaseOperator.RequestRecords requestDataDelegate)
        {
            InitializeComponent();

            RequestData = requestDataDelegate;
            BindNewDataToGrid();
        }

        private async void BindNewDataToGrid(int? startingId = null, int count = startingIdIncrement)
        {
            LoadingMode(true);
            {
                DataTable dataTable = await RequestData(startingId, count);
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

        private void Button_Click_ToFirst(object sender, RoutedEventArgs e)
        {
            BindNewDataToGrid(1);
        }

        private void Button_Click_Previous(object sender, RoutedEventArgs e)
        {
            if (currentStartingId <= 1) return;
            else if (currentStartingId == null) BindNewDataToGrid();
            else BindNewDataToGrid(Math.Max(1, (int)currentStartingId - startingIdIncrement));
        }

        private void Button_Click_Navigate(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TheSelectIdTextBox.Text, out int id))
            {
                BindNewDataToGrid(Math.Max(1, id));
            }
            else { TheSelectIdTextBox.Text = ""; }
        }

        private void Button_Click_Next(object sender, RoutedEventArgs e)
        {
            if (currentStartingId == null) BindNewDataToGrid();
            else BindNewDataToGrid((int)currentStartingId + startingIdIncrement);
        }

        private void Button_Click_Last(object sender, RoutedEventArgs e)
        {
            BindNewDataToGrid();
        }

        private void TheSelectIdTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return) { Button_Click_Navigate(sender, e); }
        }
    }
}
