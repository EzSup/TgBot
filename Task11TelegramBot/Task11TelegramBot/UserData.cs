using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task11TelegramBot
{
    public class UserData
    {
        public CultureInfo Culture;
        public string DateTemplate;
        public UserData()
        {
            Culture = new CultureInfo("en-GB");
            DateTemplate = Culture.DateTimeFormat.ShortDatePattern;
            ExchangeSearchingLogic.FormatDatePattern(ref DateTemplate);
        }
    }
}
