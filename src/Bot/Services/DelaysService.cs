using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PlainHttp;

namespace Bot.Services
{
    public class DelaysService : IDelaysService
    {
        private const string BASE_URL = "https://app-tpl.tndigit.it/gtlservice/trips/";
        private readonly IMemoryCache cache;
        private readonly ILogger<DelaysService> logger;

        public DelaysService(IMemoryCache cache, ILogger<DelaysService> logger)
        {
            this.cache = cache;
            this.logger = logger;
        }

        public async Task<double?> GetDelay(string tripId)
        {
            string cacheKey = $"delays/{tripId}";

            if (this.cache.TryGetValue(cacheKey, out double cachedDelay))
            {
                this.logger.LogInformation($"Getting delay {cachedDelay} from cache {cacheKey}");
                return cachedDelay;
            }

            var request = new HttpRequest(BASE_URL + tripId)
            {
                Timeout = TimeSpan.FromSeconds(1),
                Headers = new Dictionary<string, string>
                {
                    { "User-Agent", "ExpelliarbusBot (+https://t.me/expelliarbusbot)" }
                }
            };

            double delay;
            DateTimeOffset lastEvent;

            try
            {
                HttpResponse response = await request.SendAsync();

                JsonDocument json = JsonDocument.Parse(response.Body);
                delay = json.RootElement.GetProperty("delay").GetDouble();
                lastEvent = json.RootElement.GetProperty("lastEventRecivedAt").GetDateTimeOffset();
            }
            catch
            {
                return null;
            }

            DateTimeOffset expiration = lastEvent.AddSeconds(30);
            this.logger.LogInformation($"Writing delay {delay} to {cacheKey} until {expiration}");

            this.cache.Set(cacheKey, delay, expiration);

            return delay;
        }
    }
}
