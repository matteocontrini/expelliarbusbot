using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Bot
{
    public class BotHostedService : IHostedService
    {
        /// <summary>
        /// Cancellation token source used for shutting down long polling
        /// </summary>
        private CancellationTokenSource tokenSource;

        /// <summary>
        /// The Telegram client for managing the bot
        /// </summary>
        private TelegramBotClient bot;

        /// <summary>
        /// Information about the bot
        /// </summary>
        private User me;

        /// <summary>
        /// Configuration for the bot
        /// </summary>
        private readonly BotConfiguration config;

        /// <summary>
        /// Logger instance for this context
        /// </summary>
        private readonly ILogger<BotHostedService> logger;

        public BotHostedService(IOptions<BotConfiguration> options,
                                ILogger<BotHostedService> logger)
        {
            this.config = options.Value;
            this.logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            this.bot = new TelegramBotClient(this.config.BotToken);

            // Get information about the bot associated with the token
            this.me = await this.bot.GetMeAsync();

            this.logger.LogInformation($"Running as @{this.me.Username}");

            // Register event handlers
            this.bot.OnMessage += OnMessage;
            this.bot.OnReceiveError += OnReceiveError;
            this.bot.OnReceiveGeneralError += OnReceiveGeneralError;

            // Create a new token to be passed to .StartReceiving() below.
            // When the token is canceled, the tg client stops receiving
            this.tokenSource = new CancellationTokenSource();

            // Start getting messages
            this.bot.StartReceiving(cancellationToken: this.tokenSource.Token);
        }

        private void OnReceiveGeneralError(object sender, Telegram.Bot.Args.ReceiveGeneralErrorEventArgs e)
        {
            this.logger.LogError(e.Exception, "OnReceiveGeneralError");
        }

        private void OnReceiveError(object sender, Telegram.Bot.Args.ReceiveErrorEventArgs e)
        {
            this.logger.LogError(e.ApiRequestException, "OnReceiveError");
        }

        private void OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            if (e.Message.Type == MessageType.Text)
            {
                this.logger.LogInformation("TXT <{0}> {1}", e.Message.Chat.Id, e.Message.Text);

                // Echo
                this.bot.SendTextMessageAsync(e.Message.Chat.Id, e.Message.Text);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.tokenSource.Cancel();

            return Task.CompletedTask;
        }
    }
}
