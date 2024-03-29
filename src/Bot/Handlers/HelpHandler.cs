﻿using System.Text;
using System.Threading.Tasks;
using Bot.Services;
using Telegram.Bot;
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
            builder.AppendLine("🕑 Il bot indica anche i *ritardi in tempo reale*, se disponibili. Non sono mostrate informazioni se la corsa non è ancora partita");
            builder.AppendLine();
            builder.AppendLine("🤯 Il bot è stato sviluppato da Matteo Contrini (@matteosonoio). Un ringraziamento speciale a Dario per il logo 👏");
            builder.AppendLine();
            builder.Append("🤓 Il bot è [open source](https://github.com/matteocontrini/expelliarbusbot)");

            await this.bot.Client.SendTextMessageAsync(
                chatId: this.Chat.Id,
                text: builder.ToString(),
                replyMarkup: this.bot.GetDefaultKeyboard(this.Chat.Type),
                parseMode: ParseMode.Markdown
            );
        }
    }
}
