using System.Reflection.Metadata.Ecma335;
using System.Text.Json;

namespace CurrencyChartServer
{
    static internal class DataCollector
    {

        private static HttpClient HttpClient = new();

        public static void CollectDataFromNBRB()
        {
            if (Storage.PeriodFromRequest == Storage.DataForResponse.Count) return;
            var listOfPeriod = RequestDivider1YearLimit();

            for (int i = 0; i < listOfPeriod.Count - 1; i++)
            {
                var iDs = GetSecondCurIdOrZero(listOfPeriod[i], listOfPeriod[(i + 1)], Storage.CurrencyFromRequest);
                if (iDs.Count < 1) return;
                int curId1 = iDs[0];
                var responseFromNBRBdynamic = HttpClient.GetAsync($"https://www.nbrb.by/api/exrates/rates/dynamics/{curId1}" +
                   $"?startDate={listOfPeriod[i].Date:yyyy-MM-dd}&enddate={listOfPeriod[(i + 1)].Date:yyyy-MM-dd}").Result;

                List<RateShort> curencyRatewithFirstId = JsonSerializer.Deserialize<RateShort[]>(responseFromNBRBdynamic.Content.ReadAsStream())!.ToList();

                if (iDs.Count == 2)
                {
                    var curId2 = iDs[1];
                    var responseFromNBRBdynamicSecondId = HttpClient.GetAsync($"https://www.nbrb.by/api/exrates/rates/dynamics/{curId2}" + $"?startDate={listOfPeriod[i].Date:yyyy-MM-dd}&enddate={listOfPeriod[(i + 1)].Date:yyyy-MM-dd}").Result;
                    var currencyRateWithSecondId = JsonSerializer.Deserialize<RateShort[]>(responseFromNBRBdynamicSecondId.Content.ReadAsStream())!.ToList();
                    curencyRatewithFirstId.AddRange(currencyRateWithSecondId);
                }
                var resultTOSave = curencyRatewithFirstId.ToArray();
                Console.WriteLine("Данные собраны, сохраняю и отправляю запрос.");
                Storage.SaveDataToCacheAndDb(Storage.CurrencyFromRequest, resultTOSave);
            }

        }
        private static List<int> GetSecondCurIdOrZero(DateTime startDate, DateTime endDate, Currency currency)
        {
            var iDs = new List<int>();
            try
            {
                var responseFromNBRBOnDay = HttpClient.GetAsync($"https://www.nbrb.by/api/exrates/rates?ondate={startDate.Date:yyyy-MM-dd}&periodicity=0").Result;
                var rateArrayFromResponse = JsonSerializer.Deserialize<Rate[]>(responseFromNBRBOnDay.Content.ReadAsStream());
                if (rateArrayFromResponse != null)
                {
                    var currencyFromResponse = rateArrayFromResponse.FirstOrDefault(rate => rate.Cur_Abbreviation == currency.Cur_Abbreviation);
                    int curId;
                    if (currencyFromResponse != null)
                    {
                        curId = currencyFromResponse.Cur_ID;
                        iDs.Add(curId);
                        var responseFromNBRBForCurrencyCheck = HttpClient.GetAsync($"https://www.nbrb.by/api/exrates/currencies/{curId}").Result;
                        var currentCurrencyWithInfo = JsonSerializer.Deserialize<Currency>(responseFromNBRBForCurrencyCheck.Content.ReadAsStream());
                        if (currentCurrencyWithInfo != null && currentCurrencyWithInfo.Cur_DateEnd < endDate.Date)
                        {
                            var responselistOfCurrencies = HttpClient.GetAsync($"https://www.nbrb.by/api/exrates/currencies/").Result;
                            var listOfCurrencies = JsonSerializer.Deserialize<Currency[]>(responselistOfCurrencies.Content.ReadAsStream());
                            var currencyWithSecondId = listOfCurrencies?.FirstOrDefault(currency => currency.Cur_ParentID == currentCurrencyWithInfo.Cur_ParentID
                                && currency.Cur_DateStart > currentCurrencyWithInfo.Cur_DateEnd);
                            if (currencyWithSecondId != null)
                            {
                                int secondId = currencyWithSecondId.Cur_ID;
                                iDs.Add(secondId);
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Проверьте интернет подключение");
                }

            }
            catch (Exception)
            {

                Console.WriteLine("Проверьте интернет подключение");
            }
            return iDs;
        }
        private static List<DateTime> RequestDivider1YearLimit()
        {
            var periodForNBRB = new List<DateTime>
            {
                Storage.StartDateFromRequest,
                Storage.EndDateFromRequest,
            };


            int countOfDaysInNBRBRequest = Storage.PeriodFromRequest - Storage.DataForResponse.Count;
            int countOfNBRBRequest = countOfDaysInNBRBRequest / 365 + 1;
            for (int i = 1; i < countOfNBRBRequest; i++)
            {
                Storage.StartDateFromRequest = Storage.StartDateFromRequest.AddYears(1);
                periodForNBRB.Insert(i, Storage.StartDateFromRequest);
            }
            return periodForNBRB;
        }
    }
}
