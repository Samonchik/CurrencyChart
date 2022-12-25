using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CurrencyChartServer
{
    public class Currency
    {
        public int Cur_ID { get; set; }
        public int? Cur_ParentID { get; set; }
        public string? Cur_Abbreviation { get; set; }
        public DateTime Cur_DateStart { get; set; }
        public DateTime Cur_DateEnd { get; set; }
    }
}