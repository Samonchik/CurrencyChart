using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
namespace CurrencyChartServer
{
   static class Program
    {
        static private bool keepRunning = true;
        static void Main(string[] args)
        {
            Console.CancelKeyPress += delegate (object? sender, ConsoleCancelEventArgs e)
            {
                e.Cancel = true;
                keepRunning = false;
            };


            var Server = new Server();
            Server.Start();
            while (keepRunning) { }
            Server.Stop();
        }
    }
}