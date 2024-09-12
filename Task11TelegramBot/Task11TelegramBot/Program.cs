using Microsoft.VisualBasic;
using System;
using System.Globalization;
using System.Net;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Task11TelegramBot
{
    internal class Program
    {
        static readonly HttpClient _client = new HttpClient();
        private static string _startMessageText = "Hello, this bot has data on foreign exchange rates in UAH based on information from July 1, 2014 to the present day.";
        private static string _infoMessageText = "Try \"/culture\" to pick another date output style \nor enter currency code and date according to your culture";
        private static Dictionary<string, UserData> _usersData = new();


        static async Task Main(string[] args)
        {
            string configPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            var cofig = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(configPath, optional: true, reloadOnChange: false)
                .Build();

            var botClient = new TelegramBotClient(cofig.GetConnectionString("BotToken"));

            using CancellationTokenSource cts = new();

            ReceiverOptions receiverOptions = new()
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };

            botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
        );

            var me = await botClient.GetMeAsync();

            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();

            // Send cancellation request to stop bot
            cts.Cancel();
        }

        /// <summary>
        /// Processes received messages
        /// </summary>
        static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message is not { } message)
                return;

            if (message.Text is not { } messageText)
                return;

            var chatId = message.Chat.Id;

            ExchangeSearchingLogic LocalSearchingLogic;

            Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");
            if (!_usersData.TryGetValue(chatId.ToString(), out var userData))
            {
                _usersData.Add(chatId.ToString(), new UserData());
                _usersData.TryGetValue(chatId.ToString(), out userData);
                LocalSearchingLogic = new(userData);
                Console.WriteLine($"New user was added. Key: {chatId}");
            }
            else
            {
                LocalSearchingLogic = new ExchangeSearchingLogic(userData);
            }
            if (LocalSearchingLogic.Cultures.TryGetValue(messageText, out var culture))
            {
                LocalSearchingLogic.SetCulture(culture);
                Console.WriteLine($"Culture \"{LocalSearchingLogic.UserData.Culture}\" was set to {chatId}");
                var dateTime = DateTime.Now;
                await HideReplyKeyboardCultureSelection(LocalSearchingLogic, botClient, chatId, cancellationToken);
                Console.WriteLine($"Culture selection menu was hidden in {chatId}");
                var format = LocalSearchingLogic.UserData.Culture.DateTimeFormat.ShortDatePattern;
                ExchangeSearchingLogic.FormatDatePattern(ref format);
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"Send message in format \"USD {dateTime.ToString(format)}\"",
                    cancellationToken: cancellationToken);
            }
            else if (messageText == "/start")
            {
                await SendStartMessage(botClient, cancellationToken, chatId);
            }
            else if (messageText == "/culture")
            {
                await ShowReplyKeyboardCultureSelection(LocalSearchingLogic,botClient, update, cancellationToken, chatId);
                Console.WriteLine($"Culture selection menu was shown in {chatId}");
            }
            else
            {
                try
                {
                    LocalSearchingLogic.ParseMessage(messageText, out var code, out var date);
                    ExchangeRateRecord rate = await LocalSearchingLogic.FindAndPrintExchangeRateAsync(date, code, _client);
                    if (rate != null)
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: rate.ToString(LocalSearchingLogic.UserData.Culture),
                            cancellationToken: cancellationToken
                            );
                        Console.WriteLine($"Record by this data: {messageText} was found and written to {chatId}");
                    }
                }
                catch (Exception ex)
                {
                    await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: ex.Message,
                    cancellationToken: cancellationToken);
                    SendInfoMessage(botClient, cancellationToken, chatId);
                }
            }
        }

        /// <summary>
        /// Sends notifications about bot capabilities
        /// </summary>
        static async Task SendInfoMessage(ITelegramBotClient botClient, CancellationToken cancellationToken, long chatId)
        {
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: _infoMessageText,
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Sends a start message
        /// </summary>
        static async Task SendStartMessage(ITelegramBotClient botClient, CancellationToken cancellationToken, long chatId)
        {
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"{_startMessageText}\n{_infoMessageText}",
                cancellationToken: cancellationToken
                );
        }

        /// <summary>
        /// Handles bot errors
        /// </summary>
        static Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Displays the menu for selecting a culture
        /// </summary>
        static async Task ShowReplyKeyboardCultureSelection(ExchangeSearchingLogic searchingLogic, ITelegramBotClient botClient, Update update, CancellationToken cancellationToken, long chatId)
        {
            List<KeyboardButton> KeyboardButtons = new();
            foreach (var key in searchingLogic.Cultures.Keys)
            {
                KeyboardButtons.Add(new KeyboardButton(key.ToString()));
            }
            var replyMarkup = new ReplyKeyboardMarkup(KeyboardButtons.ToArray()) { ResizeKeyboard = true };

            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Select culture",
                replyMarkup: replyMarkup,
                cancellationToken: cancellationToken
                );
        }

        /// <summary>
        /// Hides the menu for selecting a culture
        /// </summary>
        static async Task HideReplyKeyboardCultureSelection(ExchangeSearchingLogic searchingLogic, ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
        {
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"Culture set as \"{searchingLogic.UserData.Culture}\"",
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: cancellationToken);
        }
    }
}