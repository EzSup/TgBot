using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Task11TelegramBot.ParsingObjects
{
    public class JsonExchangeRateData
    {
        [JsonPropertyName("01.12.2014")]
        public DateTime Date { get; set; }
        [JsonPropertyName("PB")]
        public string Bank { get; set; }
        [JsonPropertyName("baseCurrency")]
        public int BaseCurrency { get; set; }
        [JsonPropertyName("baseCurrencyLit")]
        public string BaseCurrencyLit { get; set; }
        [JsonPropertyName("exchangeRate")]
        public List<JsonExchangerateRate> ExchangeRateList { get; set; }
        
        public JsonExchangerateRate GetRate(string code)
        {
            try
            {
                return ExchangeRateList.First(rate => rate.Currency == code)
                    ?? throw new ArgumentException("Currency with this code was not found");
            }
            catch
            {
                throw new ArgumentException("Currency with this code was not found");
            }
        }
    }    
}
