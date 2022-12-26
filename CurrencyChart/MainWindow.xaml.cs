using CurrencyChart.Properties;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime;
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
            Left = Properties.Settings.Default.MainWindowLeft;
            Top = Properties.Settings.Default.MainWindowTop;
            DataCollector.Logger("Настройки положения окна восстановлены");

        }
        protected override void OnClosed(EventArgs e)
        {
            Properties.Settings.Default.MainWindowLeft = Left;
            Properties.Settings.Default.MainWindowTop = Top;

            Properties.Settings.Default.Save();
            DataCollector.Logger("Настройки положения окна сохранены");
            base.OnClosed(e);
        }



        private void userStartDateSelector_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {

            UserStartDate = userStartDateSelector.SelectedDate;
            DataCollector.Logger($"Дата начала периода {UserStartDate:yyyy-MM-dd} сохранена для формирования запроса на сервер");
        }

        private void userEndDateSelector_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            UserEndDate = userEndDateSelector.SelectedDate;
            DataCollector.Logger($"Дата конца периода {UserEndDate:yyy-MM-dd} сохранена для формирования запроса на сервер");
        }

        private void userCurrencySelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (userCurrencySelector.SelectedItem != null)
            {

                CurrencyName = ((TextBox)userCurrencySelector.SelectedItem).Text;
            }
            DataCollector.Logger($"Валюта {CurrencyName} сохранена для формирования запроса на сервер");
        }
        private async Task BuildChartAsync()
        {
            if (CurrencyName != null)
            {
                var currencyFromServer = await DataCollector.CollectCurrencyChartServerAsync(CurrencyName, UserStartDate, UserEndDate);
                if (currencyFromServer != null)
                {
                    var dateX = DataCollector.ParseDataForChart(currencyFromServer).Select(x => x.ToOADate()).ToArray();
                    DataCollector.Logger("Данные оси X сформированы");
                    var valueY = DataCollector.ParseRateForChart(currencyFromServer);
                    DataCollector.Logger("Данные оси Y сформированы");
                    currencyChart.Plot.Clear();
                    DataCollector.Logger("Предыдущий график очищен");
                    currencyChart.Plot.AddScatter(dateX, valueY);
                    currencyChart.Plot.XAxis.DateTimeFormat(true);
                    currencyChart.Plot.YAxis2.SetSizeLimit(min: 40);

                    if (CurrencyName == "BTC")
                    {
                        currencyChart.Plot.Title($"Курс {CurrencyName} к USD за период с {UserStartDate:yyyy-MM-dd} по {UserEndDate:yyyy-MM-dd}");
                    }
                    else
                    {
                        currencyChart.Plot.Title($"Курс {CurrencyName} к BYN за период с {UserStartDate:yyyy-MM-dd} по {UserEndDate:yyyy-MM-dd}");
                    }

                    currencyChart.Refresh();
                    DataCollector.Logger("График построен");
                }
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (UserEndDate < UserStartDate)
            {
                MessageBox.Show("Дата окончания не может быть меньше, чем дата начала.");
                DataCollector.Logger("Некорректная дата начала перехвачена");
                return;
            }
            await BuildChartAsync();

        }
    }
}
