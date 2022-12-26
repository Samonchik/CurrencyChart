using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CurrencyChartServer
{
    internal class CustomDateTimeConverter : JsonConverter<DateTime>
    {
        private readonly string Format;
        public CustomDateTimeConverter(string format)
        {
            Format = format;
        }
        public override void Write(Utf8JsonWriter writer, DateTime date, JsonSerializerOptions options)
        {
            writer.WriteStringValue(date.ToString(Format));
        }
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            DateTime result = new();
            if (reader.GetString() != null)
            {
                result = DateTime.ParseExact(reader.GetString(), Format, null);
            }
            return result;
        }
    }
}
