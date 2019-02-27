using Data;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot
{
    public class BotHostedService : IHostedService
    {
        /// <summary>
        /// Cancellation token source used for shutting down long polling
        /// </summary>
        private CancellationTokenSource tokenSource;

        /// <summary>
        /// The Telegram client for managing the bot
        /// </summary>
        private TelegramBotClient bot;

        /// <summary>
        /// Information about the bot
        /// </summary>
        private User me;

        /// <summary>
        /// Configuration for the bot
        /// </summary>
        private readonly BotConfiguration config;

        /// <summary>
        /// Logger instance for this context
        /// </summary>
        private readonly ILogger<BotHostedService> logger;

        /// <summary>
        /// Data repository for getting information about trips and stops
        /// </summary>
        private readonly ITripRepository tripsRepository;

        /// <summary>
        /// Names of the stops that should be shown as primary stops to the user
        /// </summary>
        private readonly HashSet<string> interestingStops;

        /// <summary>
        /// Names of additional stops to show after the primary stops
        /// </summary>
        private readonly HashSet<string> additionalStops;

        /// <summary>
        /// Mapping between numbers (1-6 integers) and emojis
        /// </summary>
        private readonly Dictionary<int, string> numbersEmojis;

        public BotHostedService(IOptions<BotConfiguration> options,
                                ILogger<BotHostedService> logger,
                                ITripRepository tripsRepository)
        {
            this.config = options.Value;
            this.logger = logger;
            this.tripsRepository = tripsRepository;
            
            this.interestingStops = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Povo Piazza Manci",
                "Povo \"Centro Civico\"",
                "Povo Pantè",
                "Povo \"Polo Scientifico\" Ovest",
                "Povo \"Fac. Scienze\"",
                "Povo Valoni"
            };

            this.additionalStops = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Mesiano \"Fac. Ingegneria\"",
                "Piazza Dante \"Dogana\""
            };

            this.numbersEmojis = new Dictionary<int, string>
            {
                { 1, "1️⃣" },
                { 2, "2️⃣" },
                { 3, "3️⃣" },
                { 4, "4️⃣" },
                { 5, "5️⃣" },
                { 6, "6️⃣" }
            };
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            this.bot = new TelegramBotClient(this.config.BotToken);

            // Get information about the bot associated with the token
            this.me = await this.bot.GetMeAsync();

            this.logger.LogInformation($"Running as @{this.me.Username}");

            // Register event handlers
            this.bot.OnMessage += OnMessage;
            this.bot.OnCallbackQuery += OnCallbackQuery;
            this.bot.OnReceiveError += OnReceiveError;
            this.bot.OnReceiveGeneralError += OnReceiveGeneralError;

            // Create a new token to be passed to .StartReceiving() below.
            // When the token is canceled, the tg client stops receiving
            this.tokenSource = new CancellationTokenSource();

            // Start getting messages
            this.bot.StartReceiving(cancellationToken: this.tokenSource.Token);
        }

        private async void OnCallbackQuery(object sender, Telegram.Bot.Args.CallbackQueryEventArgs e)
        {
            this.logger.LogInformation("CB <{0}> {1}", e.CallbackQuery.Message.Chat.Id, e.CallbackQuery.Data);

            await HandleRouteRequest(
                chatId: e.CallbackQuery.Message.Chat.Id,
                messageId: e.CallbackQuery.Message.MessageId,
                callbackQueryId: e.CallbackQuery.Id,
                selectedTripId: e.CallbackQuery.Data
            );
        }

        private void OnReceiveGeneralError(object sender, Telegram.Bot.Args.ReceiveGeneralErrorEventArgs e)
        {
            this.logger.LogError(e.Exception, "OnReceiveGeneralError");
        }

        private void OnReceiveError(object sender, Telegram.Bot.Args.ReceiveErrorEventArgs e)
        {
            this.logger.LogError(e.ApiRequestException, "OnReceiveError");
        }

        private async void OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            if (e.Message.Type == MessageType.Text)
            {
                this.logger.LogInformation("TXT <{0}> {1}", e.Message.Chat.Id, e.Message.Text);

                await HandleTextMessage(e.Message);
            }
        }

        private async Task HandleTextMessage(Message message)
        {
            string t = message.Text;

            if (t == "/start" || t.StartsWith("/start "))
            {
                await HandleStart(message.Chat);
            }
            // TODO: improve this...
            else if (t.Contains("povo-trento", StringComparison.OrdinalIgnoreCase))
            {
                await HandleRouteRequest(message.Chat.Id, null, null, null);
            }
            else
            {
                // TODO: ?
                await HandleStart(message.Chat);
            }
        }

        private async Task HandleRouteRequest(long chatId, int? messageId, string callbackQueryId, string selectedTripId)
        {
            // Get the trips for today
            List<Trip> trips = await this.tripsRepository.GetAllTripsForToday();

            int selectedTripIndex;
            Trip selectedTrip;
            if (selectedTripId != null)
            {
                // Get specific trip with ID
                selectedTripIndex = trips.FindIndex(x => x.TripId == selectedTripId);
                selectedTrip = trips[selectedTripIndex];
            }
            else
            {
                // Get the first trip after the current time
                string now = DateTime.Now.ToString("HH:mm:ss");

                selectedTripIndex = trips.FindIndex(x => x.DepartureTime.CompareTo(now) >= 0);
                selectedTrip = trips[selectedTripIndex];
            }

            // No more trips for today (or something went very wrong?)
            if (selectedTrip == null)
            {
                return;
            }

            // Get the list of stops with times from the db
            List<StopTime> stops = await tripsRepository.GetTrip(selectedTrip.TripId);
            
            // Build the output message caption
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("*Orari alle fermate:*");
            builder.AppendLine();

            int index = 1;
            foreach (StopTime s in stops)
            {
                if (interestingStops.Contains(s.StopName))
                {
                    string prefix = this.numbersEmojis[index];
                    string time = s.DepartureTime.Substring(0, s.DepartureTime.LastIndexOf(':'));

                    builder.AppendLine($"{prefix} {time} ({s.StopName})");

                    index++;
                }
                else if (additionalStops.Contains(s.StopName))
                {
                    string time = s.DepartureTime.Substring(0, s.DepartureTime.LastIndexOf(':'));

                    builder.AppendLine($"▶ {time} ({s.StopName})");

                    index++;
                }
            }
            List<InlineKeyboardButton> kb = new List<InlineKeyboardButton>();

            if (selectedTripIndex > 0)
            {
                kb.Add(InlineKeyboardButton.WithCallbackData("◄ Prec", trips[selectedTripIndex - 1].TripId));
            }

            if (selectedTripIndex < trips.Count - 1)
            {
                kb.Add(InlineKeyboardButton.WithCallbackData("Succ ►", trips[selectedTripIndex + 1].TripId));
            }

            string mapPath = $"maps/{selectedTrip.ShapeId}.png";

            // TODO: cache the file_id instead of uploading the image every time

            using (Stream stream = System.IO.File.OpenRead(mapPath))
            {
                if (messageId.HasValue)
                {
                    InputMedia media = new InputMedia(stream, mapPath);

                    await this.bot.EditMessageMediaAsync(
                        chatId: chatId,
                        messageId: messageId.Value,
                        media: new InputMediaPhoto(media)
                        {
                            Caption = builder.ToString(),
                            ParseMode = ParseMode.Markdown
                        },
                        replyMarkup: new InlineKeyboardMarkup(kb)
                    );

                    await this.bot.AnswerCallbackQueryAsync(
                        callbackQueryId: callbackQueryId,
                        text: "✅"
                    );
                }
                else
                {
                    await this.bot.SendPhotoAsync(
                        chatId: chatId,
                        photo: stream,
                        caption: builder.ToString(),
                        parseMode: ParseMode.Markdown,
                        replyMarkup: new InlineKeyboardMarkup(kb)
                    );
                }
            }
        }

        private async Task HandleStart(Chat chat)
        {
            var msgs = new string[]
            {
                $"🔍 Ciao, {this.me.Username} è il bot sperimentale per consultare gli orari della *linea 5 da Povo a Trento*",
                "🕑 La fermata Polo Scientifico Ovest ha una leggera priorità. In alternativa viene preso come riferimento l'orario di Povo Valoni",
                "👀 Ora premi il pulsante qua sotto 👇"
            };

            foreach (string message in msgs)
            {
                await this.bot.SendTextMessageAsync(
                    chatId: chat.Id,
                    text: message,
                    replyMarkup: new ReplyKeyboardMarkup(
                        new KeyboardButton[] { new KeyboardButton("5️⃣ Povo-Trento") },
                        resizeKeyboard: true
                    ),
                    parseMode: ParseMode.Markdown
                );
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.tokenSource.Cancel();

            return Task.CompletedTask;
        }
    }
}
