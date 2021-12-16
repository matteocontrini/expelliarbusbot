using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bot.Exceptions;
using Bot.Services;
using Data;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NodaTime;
using NodaTime.Text;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.Handlers
{
    public class BusRouteHandler : HandlerBase
    {
        private readonly IBotService bot;
        private readonly ITripRepository tripRepository;

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

        /// <summary>
        /// Mapping between local image files paths and remote Telegram `file_id`s.
        /// Used to avoid to reupload the same image every time.
        /// </summary>
        private readonly IMemoryCache fileIdsCache;

        private readonly IDelaysService delaysService;
        private readonly ILogger<BusRouteHandler> logger;

        public BusRouteHandler(IBotService botService,
            ITripRepository tripRepository,
            IMemoryCache cache,
            IDelaysService delaysService,
            ILogger<BusRouteHandler> logger)
        {
            this.bot = botService;
            this.tripRepository = tripRepository;
            this.fileIdsCache = cache;
            this.delaysService = delaysService;
            this.logger = logger;

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
                "Venezia \"Port'Aquila\"",
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

        private string GetCurrentTime()
        {
            Instant now = SystemClock.Instance.GetCurrentInstant();
            DateTimeZone tz = DateTimeZoneProviders.Tzdb["Europe/Rome"];
            LocalTime time = now.InZone(tz).TimeOfDay;
            LocalTimePattern pattern = LocalTimePattern.CreateWithInvariantCulture("HH:mm:ss");
            return pattern.Format(time);
        }

        public override async Task Run()
        {
            string requestedTripId = null;

            if (this.CallbackQuery != null)
            {
                string[] data = this.CallbackQuery.Data.Split(';');

                if (data[0] == "load")
                {
                    requestedTripId = data[1];
                }
            }

            // Get the trips for today
            List<Trip> trips = await this.tripRepository.GetAllTripsForToday();

            int selectedTripIndex;
            Trip selectedTrip;

            if (requestedTripId != null)
            {
                // Get specific trip with ID
                selectedTripIndex = trips.FindIndex(x => x.TripId == requestedTripId);
                selectedTrip = trips.ElementAtOrDefault(selectedTripIndex);
            }
            else
            {
                // Get the first trip after the current time
                string now = GetCurrentTime();

                selectedTripIndex = trips.FindIndex(x => x.DepartureTime.CompareTo(now) >= 0);
                selectedTrip = trips.ElementAtOrDefault(selectedTripIndex);

                // If we're after the last trip of the day, show the last one
                if (selectedTrip == null && trips.Count > 0)
                {
                    selectedTripIndex = trips.Count - 1;
                    selectedTrip = trips[selectedTripIndex];
                }
            }

            // Wrong trip ID or missing data?
            if (selectedTrip == null)
            {
                if (this.CallbackQuery != null)
                {
                    await this.bot.Client.AnswerCallbackQueryAsync(
                        callbackQueryId: this.CallbackQuery.Id,
                        text: "❗ Nessuna corsa disponibile",
                        showAlert: true
                    );
                }
                else
                {
                    await this.bot.Client.SendTextMessageAsync(
                        chatId: this.Chat.Id,
                        text: "❗ Nessuna corsa disponibile"
                    );
                }

                return;
            }

            this.logger.LogInformation("Sending trip {TripId}", selectedTrip.TripId);

            // Get the list of stops with times from the db
            List<StopTime> stops = await this.tripRepository.GetTrip(selectedTrip.TripId);

            // Build the output message caption
            StringBuilder caption = new StringBuilder();

            caption.AppendLine($"*Corsa {selectedTripIndex + 1} di {trips.Count}:*");
            caption.AppendLine();

            int index = 1;
            foreach (StopTime s in stops)
            {
                if (this.interestingStops.Contains(s.StopName))
                {
                    string prefix = this.numbersEmojis[index];
                    string time = s.DepartureTime.Substring(0, s.DepartureTime.LastIndexOf(':'));

                    caption.AppendLine($"{prefix} {time} ({s.StopName})");

                    index++;
                }
                else if (this.additionalStops.Contains(s.StopName))
                {
                    string time = s.DepartureTime.Substring(0, s.DepartureTime.LastIndexOf(':'));

                    caption.AppendLine($"▶️ {time} ({s.StopName})");

                    index++;
                }
            }

            try
            {
                DelayResponse result = await this.delaysService.GetDelay(selectedTrip.TripId);
                double delay = result.Delay;

                caption.AppendLine();

                if (delay == 0)
                {
                    caption.Append("✅ *IN ORARIO*");
                }
                else if (delay > 0)
                {
                    caption.Append("⚠️ *RITARDO ");
                    caption.Append(delay);
                    caption.Append(delay == 1 ? " MINUTO*" : " MINUTI*");
                    if (delay >= 10)
                    {
                        caption.Append(" 😱");
                    }
                }
                else
                {
                    delay = Math.Abs(delay);
                    caption.Append("✅ *ANTICIPO ");
                    caption.Append(delay);
                    caption.Append(delay == 1 ? " MINUTO*" : " MINUTI*");
                }

                StopTime currentStop = stops.Find(x => x.StopId == result.PreviousStopId.ToString());

                if (currentStop != null)
                {
                    caption.AppendLine();
                    //caption.Append("🚦 ");
                    caption.Append("Attualmente a ");
                    caption.Append(currentStop.StopName);
                }
            }
            catch (EndOfRouteException)
            {
                caption.AppendLine();
                caption.Append("✅ *CAPOLINEA*");
            }
            catch (DataNotAvailableException)
            {
            }

            List<InlineKeyboardButton> row = new List<InlineKeyboardButton>();

            if (selectedTripIndex > 0)
            {
                row.Add(
                    InlineKeyboardButton.WithCallbackData(
                        text: "◀️",
                        callbackData: "load;" + trips[selectedTripIndex - 1].TripId
                    )
                );
            }

            row.Add(
                InlineKeyboardButton.WithCallbackData(
                    text: "🔄",
                    callbackData: $"load;{selectedTrip.TripId}"
                )
            );

            row.Add(
                InlineKeyboardButton.WithCallbackData(
                    text: "🔽",
                    callbackData: "reload;400" // 400 is the route ID for line 5
                )
            );

            if (selectedTripIndex < trips.Count - 1)
            {
                row.Add(
                    InlineKeyboardButton.WithCallbackData(
                        text: "▶️",
                        callbackData: "load;" + trips[selectedTripIndex + 1].TripId
                    )
                );
            }

            InlineKeyboardMarkup kb = new InlineKeyboardMarkup(row);

            string mapPath = $"maps/{selectedTrip.ShapeId}.png";
            string cachedFileId = this.fileIdsCache.Get<string>(mapPath);

            if (cachedFileId != null)
            {
                // Use the cached Telegram file_id
                await SendOrUpdate(null, cachedFileId, caption.ToString(), kb);
            }
            else
            {
                // Read the file from system
                using (Stream stream = System.IO.File.OpenRead(mapPath))
                {
                    string fileId = await SendOrUpdate(stream, null, caption.ToString(), kb);

                    // Save file_id to cache
                    if (fileId != null)
                    {
                        this.fileIdsCache.Set(mapPath, fileId);
                    }
                }
            }
        }

        private async Task<string> SendOrUpdate(
            Stream stream,
            string fileId,
            string caption,
            InlineKeyboardMarkup kb)
        {
            Message sentMessage;

            // Should edit existing message
            if (this.CallbackQuery != null)
            {
                InputMedia media;

                // Reuse the cached file_id
                if (fileId != null)
                {
                    media = new InputMedia(fileId);
                }
                // Use the file system stream
                else
                {
                    media = new InputMedia(stream, "file.png");
                }

                try
                {
                    // Replace the existing message (photo and caption)
                    sentMessage = await this.bot.Client.EditMessageMediaAsync(
                        chatId: this.Chat.Id,
                        messageId: this.CallbackQuery.Message.MessageId,
                        media: new InputMediaPhoto(media)
                        {
                            Caption = caption,
                            ParseMode = ParseMode.Markdown
                        },
                        replyMarkup: kb
                    );
                }
                catch (ApiRequestException exception) when (exception.Message.Contains("message is not modified"))
                {
                    await this.bot.Client.AnswerCallbackQueryAsync(
                        callbackQueryId: this.CallbackQuery.Id,
                        text: "✅ Nessun aggiornamento"
                    );

                    return null;
                }

                await this.bot.Client.AnswerCallbackQueryAsync(this.CallbackQuery.Id);
            }
            // Send a new message
            else
            {
                // Use the file stream or the cached file_id
                InputOnlineFile photo = (InputOnlineFile)stream ?? fileId;

                sentMessage = await this.bot.Client.SendPhotoAsync(
                    chatId: this.Chat.Id,
                    photo: photo,
                    caption: caption,
                    parseMode: ParseMode.Markdown,
                    replyMarkup: kb
                );
            }

            // Return the file_id of the biggest image representation
            return sentMessage.Photo.Last().FileId;
        }
    }
}
