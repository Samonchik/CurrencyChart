using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Json;
using System.Text.Json;
using System.Windows;

namespace CurrencyChart
{
    internal static class DataCollector
    {
        private static HttpClient httpClient = new() { Timeout = TimeSpan.FromSeconds(3) };
        public static async Task <RateforCash[]?> CollectCurrencyChartServerAsync(string abrOfCur, DateTime? startdate, DateTime? enddate)
        {
            RateforCash[]? graphForChart = null;
           
            try
            {
                var responseFromNBRBdynamic = await httpClient.GetAsync($"http://127.0.0.1:8228/?abr={abrOfCur}&dateone={startdate:dd.MM.yyyy}&date2={enddate:dd.MM.yyyy}");
                if (responseFromNBRBdynamic.Content != null)
                {
                    graphForChart = await JsonSerializer.DeserializeAsync<RateforCash[]>(responseFromNBRBdynamic.Content.ReadAsStream());
                }
                else
                {
                    MessageBox.Show("Пустой ответ от сервера, проверьте интернет подключение");
                }
            }
            catch (Exception)
            {

                MessageBox.Show("Сервер отключен, проверьте интернет подключение");
            }
            return graphForChart;
        }
        public static DateTime[] ParseDataForChart(RateforCash[] currencyRateFromServer)
        {
            var dateForChart = currencyRateFromServer.Select(currecny => currecny.Date).ToArray();
            return dateForChart;
           
        }
        public static double[] ParseRateForChart(RateforCash[] currencyRateFromServer)
        {
            List<double> rateForChart = new();
            var RateForChartDecimal = currencyRateFromServer.Select(currency => currency.Value).ToArray();
            foreach (var rate in RateForChartDecimal)
            {
                if (rate != null) rateForChart.Add((double)rate);
            }
            return rateForChart.ToArray();
        }

    }
}

