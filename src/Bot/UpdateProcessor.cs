using Bot.Handlers;
using Data;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Bot
{
    public class UpdateProcessor : IUpdateProcessor
    {
        private readonly ILogger logger;
        private readonly IChatRepository chatRepository;
        private readonly IHandlersFactory handlersFactory;
        private readonly IBotService bot;

        public UpdateProcessor(ILogger<UpdateProcessor> logger,
                               IChatRepository chatRepository,
                               IHandlersFactory handlersFactory,
                               IBotService botService)
        {
            this.logger = logger;
            this.chatRepository = chatRepository;
            this.handlersFactory = handlersFactory;
            this.bot = botService;
        }

        public async Task ProcessUpdate(Update update)
        {
            if (update.Type == UpdateType.Message)
            {
                await LogChat(update.Message.Chat);

                await HandleMessage(update.Message);
            }
            else if (update.Type == UpdateType.CallbackQuery)
            {
                await HandlerCallbackQuery(update.CallbackQuery);
            }
        }

        private async Task HandleMessage(Message message)
        {
            if (message.Type == MessageType.Text)
            {
                this.logger.LogInformation("TXT <{0}> {1}", message.Chat.Id, message.Text);

                await HandleTextMessage(message);
            }
            else if (message.Type == MessageType.GroupCreated)
            {
                StartHandler handler = this.handlersFactory.GetHandler<StartHandler>();
                handler.Chat = message.Chat;
                await handler.Run();
            }
            else if (message.Type == MessageType.ChatMembersAdded)
            {
                if (message.NewChatMembers.FirstOrDefault(x => x.Username == this.bot.Me.Username) != null)
                {
                    StartHandler handler = this.handlersFactory.GetHandler<StartHandler>();
                    handler.Chat = message.Chat;
                    await handler.Run();
                }
            }
            else if (message.Type == MessageType.MigratedToSupergroup)
            {
                // Remove the old instance
                await this.chatRepository.DeleteChat(message.Chat.Id);
            }
            else if (message.Type == MessageType.Text)
            {
                // Remove the bot mention in groups
                if (message.Chat.Type == ChatType.Group ||
                    message.Chat.Type == ChatType.Supergroup)
                {
                    message.Text = message.Text.Replace($"@{this.bot.Me.Username}", "");
                }

                await HandleTextMessage(message);
            }
        }

        private Task HandlerCallbackQuery(CallbackQuery callbackQuery)
        {
            this.logger.LogInformation("CB <{0}> {1}", callbackQuery.Message.Chat.Id, callbackQuery.Data);

            BusRouteHandler handler = this.handlersFactory.GetHandler<BusRouteHandler>();
            handler.Chat = callbackQuery.Message.Chat;
            handler.CallbackQuery = callbackQuery;
            return handler.Run();
        }

        private async Task HandleTextMessage(Message message)
        {
            string t = message.Text;

            if (t == "/start" || t.StartsWith("/start "))
            {
                StartHandler handler = this.handlersFactory.GetHandler<StartHandler>();
                handler.Chat = message.Chat;
                await handler.Run();
            }
            // TODO: improve recognition of buttons text
            else if (t.Contains("povo-trento", StringComparison.OrdinalIgnoreCase) ||
                     t.StartsWith("/povotrento"))
            {
                BusRouteHandler handler = this.handlersFactory.GetHandler<BusRouteHandler>();
                handler.Chat = message.Chat;
                await handler.Run();
            }
            else if (t.Contains("aiuto", StringComparison.OrdinalIgnoreCase))
            {
                HelpHandler handler = this.handlersFactory.GetHandler<HelpHandler>();
                handler.Chat = message.Chat;
                await handler.Run();
            }
            else
            {
                StartHandler handler = this.handlersFactory.GetHandler<StartHandler>();
                handler.Chat = message.Chat;
                await handler.Run();
            }
        }

        private Task LogChat(Chat chat)
        {
            ChatEntity chatEntity = new ChatEntity()
            {
                ChatId = chat.Id,
                Type = chat.Type.ToString(),
                Title = chat.Title,
                Username = chat.Username,
                FirstName = chat.FirstName,
                LastName = chat.LastName,
                UpdatedAt = DateTime.UtcNow
            };

            return this.chatRepository.InsertOrReplaceChat(chatEntity);
        }
    }
}
