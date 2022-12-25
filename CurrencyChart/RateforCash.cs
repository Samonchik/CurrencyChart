using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyChart
{
    internal class RateforCash
    {
        public string? Currency { get; set; }
        public DateTime Date { get; set; }
        public decimal? Value { get; set; }
        public int Amount { get; set; }
    }
}
