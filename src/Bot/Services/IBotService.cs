using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.Services
{
    public interface IBotService
    {
        IReplyMarkup GetDefaultKeyboard(ChatType chatType);

        TelegramBotClient Client { get; }

        User Me { get; set; }
    }
}
