using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bot
{
    public class BotService : IBotService
    {
        public TelegramBotClient Client { get; }
        public User Me { get; set;  }

        public BotService(BotConfiguration config)
        {
            this.Client = new TelegramBotClient(config.BotToken);
        }
    }
}
