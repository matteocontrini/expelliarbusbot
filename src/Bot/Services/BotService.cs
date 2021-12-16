using System;
using System.Net;
using System.Net.Http;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.Services
{
    public class BotService : IBotService
    {
        private ReplyKeyboardMarkup menuMarkup;

        public TelegramBotClient Client { get; }

        public User Me { get; set; }

        public BotService(BotConfiguration config)
        {
            HttpClient client = new HttpClient(new SocketsHttpHandler()
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(60),
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            });

            this.Client = new TelegramBotClient(config.BotToken, client);

            this.menuMarkup = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] { "5️⃣ Povo-Trento" },
                new KeyboardButton[] { "❓ Aiuto" }
            })
            {
                ResizeKeyboard = true
            };
        }

        public IReplyMarkup GetDefaultKeyboard(ChatType chatType)
        {
            if (chatType == ChatType.Private)
            {
                return this.menuMarkup;
            }
            else
            {
                return new ReplyKeyboardRemove();
            }
        }
    }
}
