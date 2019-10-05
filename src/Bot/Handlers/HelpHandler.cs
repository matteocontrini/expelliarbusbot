using System.Text;
using System.Threading.Tasks;
using Bot.Services;
using Telegram.Bot.Types.Enums;

namespace Bot.Handlers
{
    public class HelpHandler : HandlerBase
    {
        private readonly IBotService bot;

        public HelpHandler(IBotService botService)
        {
            this.bot = botService;
        }

        public override async Task Run()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"🔍 {this.bot.Me.Username} è il bot sperimentale per consultare gli orari della *linea 5 da Povo a Trento*");
            builder.AppendLine();
            builder.AppendLine("🕑 La fermata Polo Scientifico Ovest ha una leggera priorità, e in alternativa viene preso come riferimento l'orario di Povo Valoni");
            builder.AppendLine();
            builder.AppendLine("🤯 Il bot è stato sviluppato da @matteocontrini. Un ringraziamento speciale a [Dario Crisafulli](https://botfactory.it/#chisiamo) per il logo 👏");
            builder.AppendLine();
            builder.Append("🤓 Il bot è [open source](https://github.com/matteocontrini/expelliarbusbot), of course");

            await this.bot.Client.SendTextMessageAsync(
                chatId: this.Chat.Id,
                text: builder.ToString(),
                parseMode: ParseMode.Markdown
            );
        }
    }
}
