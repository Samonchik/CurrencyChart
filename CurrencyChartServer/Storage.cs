using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CurrencyChartServer
{
    internal static class Storage
    {
        internal static List<RateforCash> Db { get; set; } = new();
        internal static List<RateforCash> DataForResponse { get; set; } = new();
        public static DateTime StartDateFromRequest { get; set; }
        public static DateTime EndDateFromRequest { get; set; }
        public static Currency CurrencyFromRequest { get; set; } = new();
        public static int PeriodFromRequest { get; set; }

        private static void RestoreDataFromCache()
        {
            if (File.Exists("cache.json"))
            {
                string text = File.ReadAllText("cache.json");
                if (text != "")
                {
                    using (FileStream fs = new FileStream("cache.json", FileMode.OpenOrCreate))
                    {
                        var options = new JsonSerializerOptions { WriteIndented = true };
                        options.Converters.Add(new CustomDateTimeConverter("dd/MM/yy"));
                        var restoredRatesFromCache = JsonSerializer.Deserialize<RateforCash[]>(fs, options);
                        Db = restoredRatesFromCache!.ToList();
                    }
                }
                else
                {
                    Console.WriteLine("Файл кеша есть, но пустой.");
                }

            }
            else
            {
                using (FileStream fs = new FileStream("cache.json", FileMode.OpenOrCreate))
                {
                    Console.WriteLine("Файл кеша успешно создан.");
                }
            }
        }
        static public void CheckDataInDb()
        {
            PeriodFromRequest = EndDateFromRequest.Subtract(StartDateFromRequest).Days + 1;
            RestoreDataFromCache();
            var dataForResponseFromCache = Db
                .Where(rate => rate.Currency == CurrencyFromRequest.Cur_Abbreviation && rate.Date >= StartDateFromRequest && rate.Date <= EndDateFromRequest);

            if (PeriodFromRequest == dataForResponseFromCache.Count())
            {
                Console.WriteLine("Все данные были в кеше, запрос в НБРБ не выполнялся.");
                DataForResponse = dataForResponseFromCache.ToList();

            }
            else if (!dataForResponseFromCache.Any())
            {
                Console.WriteLine("В кеше отсуствуют данные, выполняю запрос.");
            }
            else
            {
                Console.WriteLine("Часть данных имеется в кеше, выполняю запрос.");
            }
        }
        public static void WriteDataOnFile()
        {
            var clearDbUsd = Db.Where(x => x.Currency == "USD").GroupBy(x => x.Date).Select(x => x.First()).ToList();
            var clearDbEur = Db.Where(x => x.Currency == "EUR").GroupBy(x => x.Date).Select(x => x.First()).ToList();
            var clearDbRub = Db.Where(x => x.Currency == "RUB").GroupBy(x => x.Date).Select(x => x.First()).ToList();
            var clearDbBBtc = Db.Where(x => x.Currency == "BTC").GroupBy(x => x.Date).Select(x => x.First()).ToList();
            Db.Clear();
            Db.AddRange(clearDbUsd);
            Db.AddRange(clearDbEur);
            Db.AddRange(clearDbRub);
            Db.AddRange(clearDbBBtc);
            Db = Db.OrderBy(x => x.Date).ToList();
            DataForResponse = DataForResponse.Distinct().ToList();
            var options = new JsonSerializerOptions { WriteIndented = true };
            options.Converters.Add(new CustomDateTimeConverter("dd/MM/yy"));
            using (FileStream fs = new("cache.json", FileMode.OpenOrCreate))
            {
                JsonSerializer.Serialize(fs, Db, options);
                Console.WriteLine("Кеш сохранен в файл.");
            }
        }
        public static void ConvertReciveCurrencyToCachFormat(Currency currency, RateShort[] rateShortsToSave)
        {
            var rateForCash = new List<RateforCash>();
            foreach (var rate in rateShortsToSave)
            {
                rateForCash.Add(new RateforCash
                {
                    Currency = currency.Cur_Abbreviation,
                    Date = rate.Date,
                    Value = rate.Cur_OfficialRate,
                    Amount = 1
                });
            }
            Db.AddRange(rateForCash);
            DataForResponse.AddRange(rateForCash);
            WriteDataOnFile();
        }
        public static void ConvertReciveCurrencyToCachFormat(BtcRate[] rateShortsToSave)
        {
            var rateForCash = new List<RateforCash>();
            foreach (var rate in rateShortsToSave)
            {
                rateForCash.Add(new RateforCash
                {
                    Currency = "BTC",
                    Date = rate.time_period_start,
                    Value = rate.rate_open,
                    Amount = 1
                });
            }
            Db.AddRange(rateForCash);
            DataForResponse.AddRange(rateForCash);
            WriteDataOnFile();
        }

    }
}
