﻿using Data;
using Microsoft.Extensions.Caching.Memory;
using NodaTime;
using NodaTime.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public BusRouteHandler(IBotService botService,
                               ITripRepository tripRepository,
                               IMemoryCache cache)
        {
            this.bot = botService;
            this.tripRepository = tripRepository;
            this.fileIdsCache = cache;

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
            // Get the trips for today
            List<Trip> trips = await this.tripRepository.GetAllTripsForToday();

            int selectedTripIndex;
            Trip selectedTrip;

            if (this.CallbackQuery != null)
            {
                string tripId = this.CallbackQuery.Data.Split(';')[1];

                // Get specific trip with ID
                selectedTripIndex = trips.FindIndex(x => x.TripId == tripId);
                selectedTrip = trips[selectedTripIndex];
            }
            else
            {
                // Get the first trip after the current time
                string now = GetCurrentTime();

                selectedTripIndex = trips.FindIndex(x => x.DepartureTime.CompareTo(now) >= 0);
                selectedTrip = trips[selectedTripIndex];
            }

            // No more trips for today (or something went very wrong?)
            if (selectedTrip == null)
            {
                return;
            }

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

                    caption.AppendLine($"▶ {time} ({s.StopName})");

                    index++;
                }
            }

            List<InlineKeyboardButton> row = new List<InlineKeyboardButton>();

            if (selectedTripIndex > 0)
            {
                row.Add(
                    InlineKeyboardButton.WithCallbackData(
                        text: "◄ Prec",
                        callbackData: "load;" + trips[selectedTripIndex - 1].TripId
                    )
                );
            }

            if (selectedTripIndex < trips.Count - 1)
            {
                row.Add(
                    InlineKeyboardButton.WithCallbackData(
                        text: "Succ ►",
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
                    this.fileIdsCache.Set(mapPath, fileId);
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