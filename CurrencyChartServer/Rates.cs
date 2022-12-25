using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CurrencyChartServer
{
    public class Rate
    {
        public int Cur_ID { get; set; }
        public string? Cur_Abbreviation { get; set; }
    }

    public class RateShort
    {
        public DateTime Date { get; set; }
        public decimal? Cur_OfficialRate { get; set; }
    }

}