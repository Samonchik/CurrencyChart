using ScottPlot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CurrencyChart
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string? CurrencyName { get; set; }
        public DateTime? UserStartDate { get; set; }
        public DateTime? UserEndDate { get; set; }
        public MainWindow()
        {
            InitializeComponent();
        }


        private void userStartDateSelector_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {

            UserStartDate = userStartDateSelector.SelectedDate;
        }

        private void userEndDateSelector_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            UserEndDate = userEndDateSelector.SelectedDate;
        }

        private void userCurrencySelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (userCurrencySelector.SelectedItem != null)
            {

               CurrencyName = ((TextBox)userCurrencySelector.SelectedItem).Text;
            }
        }
        private async Task BuildChartAsync()
        {
            if (CurrencyName != null)
            {
                var currencyFromServer = await DataCollector.CollectCurrencyChartServerAsync(CurrencyName, UserStartDate, UserEndDate);
                if (currencyFromServer != null)
                {
                    var dateX = DataCollector.ParseDataForChart(currencyFromServer).Select(x => x.ToOADate()).ToArray();
                    var value = DataCollector.ParseRateForChart(currencyFromServer);
                    currencyChart.Plot.Clear();
                    currencyChart.Plot.AddScatter(dateX, value);
                    currencyChart.Plot.XAxis.DateTimeFormat(true);
                    currencyChart.Plot.YAxis2.SetSizeLimit(min: 40);
                    currencyChart.Plot.Title($"Курс {CurrencyName} к BYN за период с {UserStartDate:yyyy-MM-dd} по {UserEndDate:yyyy-MM-dd}");
                    currencyChart.Refresh();
                }
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
           if (UserEndDate < UserStartDate)
            {
                MessageBox.Show("Дата окончания не может быть меньше, чем дата начала.");
                return;
            }
            await BuildChartAsync();

        }
    }
}
