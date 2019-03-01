using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.Handlers
{
    public class StartHandler : HandlerBase
    {
        private readonly IBotService bot;

        public StartHandler(IBotService botService)
        {
            this.bot = botService;
        }

        public override async Task Run()
        {
            string[] msgs = new string[]
            {
                $"🔍 Ciao, {this.bot.Me.Username} è il bot sperimentale per consultare gli orari della *linea 5 da Povo a Trento*",
                "🕑 La fermata Polo Scientifico Ovest ha una leggera priorità, e in alternativa viene preso come riferimento l'orario di Povo Valoni",
                this.Chat.Type == ChatType.Private ? "👀 Ora premi il pulsante qua sotto 👇" : "👀 Nei gruppi, usa i comandi /povotrento e /aiuto"
            };

            foreach (string message in msgs)
            {
                await this.bot.Client.SendTextMessageAsync(
                    chatId: this.Chat.Id,
                    text: message,
                    replyMarkup: (this.Chat.Type == ChatType.Private)
                        ? new ReplyKeyboardMarkup(new KeyboardButton[]
                            {
                                new KeyboardButton("5️⃣ Povo-Trento"),
                                new KeyboardButton("❓ Aiuto")
                            },
                            resizeKeyboard: true)
                        : null,
                    parseMode: ParseMode.Markdown
                );
            }
        }
    }
}
