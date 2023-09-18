using System;
using System.Text.Json;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Task11TelegramBot.ParsingObjects;

namespace Task11TelegramBot
{
    public class ExchangeRateRecord
    {
        [JsonPropertyName("date")]
        private readonly DateTime _date;
        [JsonPropertyName("currency")]
        private readonly string _сurrencyCode;
        [JsonPropertyName("saleRateNB")]
        private readonly decimal? saleRateNB;
        [JsonPropertyName("purchaseRateNB")]
        private readonly decimal? purchaseRateNB;
        [JsonPropertyName("saleRate")]
        private readonly decimal? saleRate;
        [JsonPropertyName("purchaseRate")]
        private readonly decimal? purchaseRate;

        public ExchangeRateRecord(DateTime date, string currencyCode, decimal? saleRateNB, decimal? purchaseRateNB, decimal? saleRate, decimal? purchaseRate)
        {
            _date = date;
            _сurrencyCode = currencyCode;
            this.saleRateNB = saleRateNB;
            this.purchaseRateNB = purchaseRateNB;
            this.saleRate = saleRate;
            this.purchaseRate = purchaseRate;
        }
        public ExchangeRateRecord(DateTime date, JsonExchangerateRate rate)
        {
            _date = date;
            _сurrencyCode = rate.Currency;
            this.saleRateNB = rate.SaleRateNB;
            this.purchaseRateNB = rate.PurchaseRateNB;
            this.saleRate = rate.SaleRate;
            this.purchaseRate = rate.PurchaseRate;
        }

        public ExchangeRateRecord(string currencyCode, DateTime date) : this(date, currencyCode, null, null, null, null) { }


        public string ToString( CultureInfo culture)
        {
            StringBuilder sb= new StringBuilder();
            string format = culture.DateTimeFormat.ShortDatePattern;
            ExchangeSearchingLogic.FormatDatePattern(ref format);
            sb.AppendLine($"Date: {this._date.ToString(format)}");
            sb.AppendLine($"Currency code: {this._сurrencyCode}");
            sb.AppendLine($"Sale rate NB: {saleRateNB?.ToString("N2", culture) ?? "no data "}UAH;");
            sb.AppendLine($"Purchase rate NB: {purchaseRateNB?.ToString("N2", culture) ?? "no data "}UAH;");
            sb.AppendLine($"Sale rate: {saleRate?.ToString("N2", culture) ?? "no data "}UAH;");
            sb.AppendLine($"Purchase rate: {purchaseRate?.ToString("N2", culture) ?? "no data "}UAH;");
            return sb.ToString();
        }
    }
}
