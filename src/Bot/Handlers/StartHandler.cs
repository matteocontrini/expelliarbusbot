using System.Threading.Tasks;
using Bot.Services;
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
                "🕑 Il bot indica anche i *ritardi in tempo reale*, se disponibili. Non sono mostrate informazioni se la corsa non è ancora partita",
                this.Chat.Type == ChatType.Private ? "👀 Ora premi il pulsante qua sotto 👇" : "👀 Nei gruppi, usa i comandi /povotrento e /aiuto"
            };

            foreach (string message in msgs)
            {
                await this.bot.Client.SendTextMessageAsync(
                    chatId: this.Chat.Id,
                    text: message,
                    replyMarkup: this.bot.GetDefaultKeyboard(this.Chat.Type),
                    parseMode: ParseMode.Markdown
                );
            }
        }
    }
}
