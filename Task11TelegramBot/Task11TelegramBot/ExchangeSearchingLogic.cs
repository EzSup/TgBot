using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using Microsoft.VisualBasic;
using System.Threading;
using System.Diagnostics;
using System.Text.Json;
using Task11TelegramBot.ParsingObjects;

namespace Task11TelegramBot
{
    public class ExchangeSearchingLogic
    {

        public readonly UserData UserData;     
        public Dictionary<string, CultureInfo> Cultures = new Dictionary<string, CultureInfo>()
            {
                { "MDY(USA)",new CultureInfo("en-US")},
                { "DMY(Europe)", new CultureInfo("en-GB")}
            };
        private string _codePattern = "^[A-Z]{3}$";
        private DateTime _minDate = new DateTime(2014, 7, 1);
        private readonly string _apiUrlTemplate = "https://api.privatbank.ua/p24api/exchange_rates?json&date=";
        private readonly CultureInfo _apiCulture = new CultureInfo("en-GB");
        
        public ExchangeSearchingLogic(UserData userData)
        {
            UserData = userData;
        }

        /// <summary>
        /// Sets the received parameter as currentCulture
        /// </summary>
        /// <param name="newCulture">New culture paramenter</param>
        public void SetCulture(CultureInfo newCulture)
        {
            UserData.Culture = newCulture;
        }

        /// <summary>
        /// Parses the received message as a text with the currency code and date
        /// </summary>
        /// <param name="message">message text</param>
        /// <param name="code">Parsed code</param>
        /// <param name="date">Parsed date</param>
        public void ParseMessage(string message, out string code, out DateTime date)
        {
            message = message.Trim();
            string[] arr = message.Split(" ");
            arr = arr.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
            if (arr.Length < 2)
            {
                throw new ArgumentException("Message must consist of at least 2 words");
            }
            code = ParseCode(arr[0]);
            date = ParseDate(arr[1], UserData.Culture);
        }

        /// <summary>
        /// Parses the received text as a currency code
        /// </summary>
        /// <param name="code">Received text</param>
        /// <returns>Parsed code</returns>
        /// <exception cref="ArgumentException">Parsing exception</exception>
        public string ParseCode(string code)
        {
            code = code.ToUpper().Trim();
            if (Regex.IsMatch(code, _codePattern))
                return code;
            throw new ArgumentException("Code doesn`t match the pattern!");
        }

        /// <summary>
        /// Parses the received text as a date
        /// </summary>
        /// <param name="date">Received text</param>
        /// <param name="culture">Culture of date</param>
        /// <returns>Parsed date</returns>
        /// <exception cref="ArgumentException">Parsing exception</exception>
        public DateTime ParseDate(string date, CultureInfo culture)
        {
            date = date.Trim();
            string pattern = culture.DateTimeFormat.ShortDatePattern;
            FormatDatePattern(ref pattern);
            if (DateTime.TryParseExact(date, pattern, UserData.Culture , DateTimeStyles.None, out DateTime result))
            {
                UserData.DateTemplate = pattern;
                return result;
            }
            throw new ArgumentException("Date doesn`t match the pattern!");
        }

        /// <summary>
        /// Formats the date template
        /// </summary>
        /// <param name="pattern">Pattern to be formatted</param>
        public static void FormatDatePattern(ref string pattern)
        {
            pattern = pattern.Replace("/", ".");
            if (!pattern.Contains("dd"))
                pattern = pattern.Replace("d", "dd");
            if (!pattern.Contains("MM"))
                pattern = pattern.Replace("M", "MM");
        }

        /// <summary>
        /// Searches for a record in the API and displays the record on the screen if found it
        /// </summary>
        /// <param name="date">Record`s date</param>
        /// <param name="code">Record`s currency code</param>
        /// <exception cref="Exception">Searching exception</exception>
        public async Task<ExchangeRateRecord> FindAndPrintExchangeRateAsync(DateTime date, string code, HttpClient client)
        {
            string responceBody;
            try
            {
                string apiUrl = String.Concat(_apiUrlTemplate, date.ToString("dd.MM.yyyy"));
                using HttpResponseMessage responseMessage = await client.GetAsync(apiUrl);
                responceBody = await responseMessage.Content.ReadAsStringAsync();
                JsonExchangeRateData? data = JsonSerializer.Deserialize<JsonExchangeRateData>(responceBody) ?? throw new Exception();
                var rate = new ExchangeRateRecord(date, data?.GetRate(code));
                return rate;
                }
            catch(ArgumentException ex)
            {
                throw ex;
            }
            catch
            {
                throw new Exception($"No data found for this date. Database contain exchanges since {_minDate.ToString(UserData.DateTemplate)}");
            }
        }
    }
}
