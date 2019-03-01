using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bot
{
    public interface IBotService
    {
        TelegramBotClient Client { get; }

        User Me { get; set; }
    }
}
