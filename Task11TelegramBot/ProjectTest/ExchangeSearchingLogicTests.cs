using Task11TelegramBot;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ProjectTest
{
    [TestClass]
    public class ExchangeSearchingLogicTests
    {
        private ExchangeSearchingLogic _exchangeSearchingLogic;

        [TestInitialize]
        public void Setup()
        {
            _exchangeSearchingLogic = new ExchangeSearchingLogic(new UserData());
        }

        [DataTestMethod]
        [DataRow("USD")]
        [DataRow("eur")]
        [DataRow("CzK")]
        [DataRow("cHf")]
        [DataRow("GBP ")]
        [DataRow(" PLZ")]
        [DataRow("   RND  ")]
        public void ParseCode_CorrectValues_ReturnsCorrectResult(string enteredCode)
        {
            string expected = enteredCode.ToUpper().Trim();
            string result = _exchangeSearchingLogic.ParseCode(enteredCode);
            Assert.AreEqual(expected, result);
        }

        [DataTestMethod]
        [DataRow("us")]
        [DataRow("EURO")]
        [DataRow("krone")]
        [DataRow("zoloto")]
        [DataRow("Gold")]
        [DataRow("GOLD")]
        [DataRow("")]
        [DataRow("    ")]
        [ExpectedException(typeof(ArgumentException))]
        public void ParseCode_InvalidValues_ThrowArgEx(string enteredCode)
        {
            _exchangeSearchingLogic.ParseCode(enteredCode);
        }

        [DataTestMethod]
        [DataRow("31.12.2023", "en-GB", 2023,12,31)]
        [DataRow("01.01.2023", "en-GB", 2023, 1, 1)]
        [DataRow("12.31.2023", "en-US", 2023, 12, 31)]
        [DataRow("01.01.2023", "en-US", 2023, 1, 1)]
        [DataRow("  08.05.2005   ", "en-US", 2005, 8, 5)]
        public void ParseDate_CorrectValues_ReturnsCorrectResult(string date, string culture, int year, int month, int day)
        {
            CultureInfo cultureInfo = new CultureInfo(culture);
            DateTime expectedResult = new DateTime(year, month, day);
            DateTime result = _exchangeSearchingLogic.ParseDate(date, cultureInfo);
            Assert.AreEqual(expectedResult, result);
        }

        [DataTestMethod]
        [DataRow("32.12.2023", "en-GB")]
        [DataRow("01.13.2023", "en-GB")]
        [DataRow("1.1.2023", "en-GB")]
        [DataRow("001.01.2023", "en-GB")]
        [DataRow("12.32.2023", "en-US")]
        [DataRow("01/01/2023", "en-US")]
        [DataRow("08.32.2005", "en-US")]
        [DataRow("13.05.2005", "en-US")]
        [ExpectedException(typeof(ArgumentException))]
        public void ParseDate_InvalidValues_ThrowsArgumentException(string date, string culture)
        {
            CultureInfo cultureInfo = new CultureInfo(culture);
            _exchangeSearchingLogic.ParseDate(date, cultureInfo);
        }

        [DataTestMethod]
        [DataRow("USD 01.07.2014", "USD", 1,7,2014)]
        [DataRow("usd 07.08.2022", "USD", 7,8,2022)]
        [DataRow("    uSd       09.10.2012  ", "USD", 9,10,2012)]
        public void ParseMessage_CorrectValues_ReturnsCorrectResult(string message, string codeExpected, int day, int month, int year)
        {
            DateTime dateExpected = new(year,month,day);
            _exchangeSearchingLogic.ParseMessage(message, out var codeResult, out var dateResult);
            Assert.AreEqual(codeExpected, codeResult);
            Assert.AreEqual(dateExpected, dateResult);
        }

        [DataTestMethod]
        [DataRow("USD 29.13.2014")]
        [DataRow("USD 29.02.2014")]
        [DataRow("usd 32.08.2022")]
        [DataRow("US D 01.01.2020")]
        [ExpectedException(typeof(ArgumentException))]
        public void ParseMessage_InvalidValues_ThrowArgEx(string message)
        {
            _exchangeSearchingLogic.ParseMessage(message, out var codeResult, out var dateResult);
        }

        [DataTestMethod]
        [DataRow("d/M/yyyy","dd.MM.yyyy")]
        [DataRow("M/d/yyyy", "MM.dd.yyyy")]
        [DataRow("MM/d/yyyy", "MM.dd.yyyy")]
        [DataRow("M/dd/yyyy", "MM.dd.yyyy")]
        public void FormatDate_CorrectValues_ReturnsCorrectResult(string entered, string expected)
        {
            ExchangeSearchingLogic.FormatDatePattern(ref entered);
            Assert.AreEqual(entered, expected);
        }

        //--------------------------------------------------------------------------------------------------

        private static object RunPrivateMethod(object instance, string methodName, object[] input)
        {
            var type = instance.GetType();
            var method = type?.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            var result = method?.Invoke(instance, input);
            return result;
        }


        private static T GetPrivateField<T>(object instance, string fieldName)
        {
            var type = instance.GetType();
            var field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            return (T)field.GetValue(instance);
        }

    }
}