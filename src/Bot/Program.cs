﻿using Bot.Handlers;
using CustomConsoleLogger;
using Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Bot
{
    class Program
    {
        /// <summary>
        /// Holds the <see cref="CancellationToken"/> used
        /// for shutting down the Host
        /// </summary>
        private static CancellationTokenSource shutdownTokenSource =
            new CancellationTokenSource();

        public static async Task Main(string[] args)
        {
            // Build the host
            IHost host = new HostBuilder()
                .ConfigureAppConfiguration(ConfigureApp)
                .ConfigureServices(ConfigureServices)
                .ConfigureLogging(ConfigureLogging)
                .UseConsoleLifetime()
                .Build();

            // Get the logger
            ILogger logger = host.Services.GetRequiredService<ILogger<Program>>();

            try
            {
                await host.RunAsync(shutdownTokenSource.Token);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception");

                // Stop the WebHost, otherwise it might hang here
                shutdownTokenSource.Cancel();

                // Required to stop all the threads.
                // With "return 1", the process could actually stay online forever
                Environment.Exit(1);
            }
        }

        private static void ConfigureLogging(HostBuilderContext hostContext, ILoggingBuilder logging)
        {
            logging.AddConfiguration(hostContext.Configuration.GetSection("Logging"));
            logging.AddCustomConsole();
        }

        private static void ConfigureApp(HostBuilderContext hostContext, IConfigurationBuilder configApp)
        {
            // This loads the application settings
            configApp.SetBasePath(Directory.GetCurrentDirectory());
            configApp.AddJsonFile("appsettings.json");
        }

        private static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
        {
            // TODO CORE 3.0: Change after https://github.com/aspnet/Hosting/issues/1346 is released
            services.Configure<ConsoleLifetimeOptions>(console => console.SuppressStatusMessages = true);

            services.UseConfigurationValidation();
            
            services.ConfigureValidatableSetting<BotConfiguration>(hostContext.Configuration.GetSection("Bot"));
            services.ConfigureValidatableSetting<DatabaseConfiguration>(hostContext.Configuration.GetSection("Database"));

            services.AddMemoryCache();

            // Database things
            services.AddSingleton<ISQLiteFactory, SQLiteFactory>();
            services.AddTransient<ITripRepository, TripRepository>();
            services.AddTransient<IChatRepository, ChatRepository>();

            services.AddSingleton<IBotService, BotService>();
            services.AddScoped<IUpdateProcessor, UpdateProcessor>();
            services.AddHandlers();

            services.AddHostedService<BotHostedService>();
        }
    }
}
