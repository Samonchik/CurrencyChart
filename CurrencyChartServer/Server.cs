using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CurrencyChartServer
{
    public class Server
    {
        private readonly HttpListener listner = new HttpListener();
        public void Start()
        {
            listner.Prefixes.Add("http://127.0.0.1:8228/");
            listner.Start();
            Console.WriteLine("Сервер запущен, ожидание подключений.");
            ReciveContext();
        }
        public void Stop()
        {
            listner.Stop();
            listner.Close();
            Console.WriteLine("Сервер остановлен.");
        }
        private void ReciveContext()
        {
            listner.BeginGetContext(new AsyncCallback(ListenerCallback), listner);
        }
        private void ListenerCallback(IAsyncResult result)
        {
            if (listner.IsListening)
            {
                var context = listner.EndGetContext(result);
                var request = context.Request;
                var response = context.Response;
                WorkWithContext(request, response);
                ReciveContext();
            }
        }
        private static void WorkWithContext(HttpListenerRequest requestForWork, HttpListenerResponse responseForWork)
        {
            if (IfValidGetFieldsFromRequest(requestForWork))
            {
                Console.WriteLine($"Получен запрос курса {Storage.CurrencyFromRequest?.Cur_Abbreviation} за период с " +
                    $"{Storage.StartDateFromRequest.Date:yyyy-MM-dd} по {Storage.EndDateFromRequest.Date:yyyy-MM-dd}");
                Storage.CheckDataInDb();
                DataCollector.CollectDataFromNBRB();
                SendResponse(responseForWork);
            }
            else
            {
                Console.WriteLine("Некорректный запрос");
            }
        }
        private static bool IfValidGetFieldsFromRequest(HttpListenerRequest request)
        {
            if (request.QueryString.Count != 3) return false;
            var queryArray = request.QueryString.GetValues(0);
            var queryArray1 = request.QueryString.GetValues(1);
            var queryArray2 = request.QueryString.GetValues(2);
            Currency? currency;
            if (queryArray != null && queryArray[0] == "USD") currency = new Currency() { Cur_Abbreviation = "USD" };
            else if (queryArray != null && queryArray[0] == "EUR") currency = new Currency() { Cur_Abbreviation = "EUR" };
            else if (queryArray != null && queryArray[0] == "RUB") currency = new Currency() { Cur_Abbreviation = "RUB" };
            else return false;

            if (queryArray1 == null || !DateTime.TryParse(queryArray1[0], out DateTime startdate)) return false;

            if (queryArray2 == null || !DateTime.TryParse(queryArray2[0], out DateTime enddate)) return false;

            if (startdate < new DateTime(2017, 12, 1) || enddate > DateTime.Now) return false;
            Storage.StartDateFromRequest = startdate;
            Storage.EndDateFromRequest = enddate;
            Storage.CurrencyFromRequest = currency;
            return true;
        }
        private static void SendResponse(HttpListenerResponse responseForWork)
        {
            responseForWork.StatusCode = (int)HttpStatusCode.OK;
            responseForWork.ContentType = "json";
            var responseObject = Storage.DataForResponse
                .OrderBy(rate => rate.Date).ToArray();

            Storage.DataForResponse.Clear();
            if (responseObject.Length != Storage.PeriodFromRequest) return;
            byte[] buffer = JsonSerializer.SerializeToUtf8Bytes(responseObject, typeof(RateforCash[]));
            responseForWork.ContentLength64 = buffer.Length;
            using Stream output = responseForWork.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Flush();
            Console.WriteLine("Запрос обработан \n\n");
        }
    }
}
