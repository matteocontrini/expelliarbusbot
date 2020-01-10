using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Exceptions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using PlainHttp;

namespace Bot.Services
{
    public class DelaysService : IDelaysService
    {
        private readonly string baseUrl;
        private readonly string token;
        private readonly IMemoryCache cache;
        private readonly ILogger<DelaysService> logger;

        public DelaysService(IMemoryCache cache,
                             ILogger<DelaysService> logger,
                             FugattiConfiguration configuration)
        {
            this.baseUrl = configuration.BaseUrl;
            this.token = configuration.Token;
            this.cache = cache;
            this.logger = logger;
        }

        public async Task<DelayResponse> GetDelay(string tripId)
        {
            string cacheKey = $"delays/{tripId}";

            if (this.cache.TryGetValue(cacheKey, out DelayResponse cachedDelay))
            {
                this.logger.LogInformation($"Getting delay {cachedDelay} from cache {cacheKey}");
                return cachedDelay;
            }

            var request = new HttpRequest($"{baseUrl}/trips/{tripId}")
            {
                Timeout = TimeSpan.FromSeconds(1),
                Headers = new Dictionary<string, string>
                {
                    { "Authorization", $"Basic {token}" }
                }
            };

            double delay;
            DateTimeOffset lastEvent;
            int endOfRouteStopId;
            int previousStopId;
            int nextStopId;

            try
            {
                HttpResponse response = await request.SendAsync();

                JObject json = JObject.Parse(response.Body);

                delay = json["delay"].ToObject<double>();
                lastEvent = json["lastEventRecivedAt"].ToObject<DateTimeOffset>();
                endOfRouteStopId = json["stopTimes"].Last["stopId"].ToObject<int>();

                previousStopId = json["stopLast"].ToObject<int>();
                nextStopId = json["stopNext"].ToObject<int>();
            }
            catch
            {
                throw new DataNotAvailableException();
            }

            if (endOfRouteStopId == nextStopId)
            {
                // ^ use stopNext because sometimes the last stop is never reached
                throw new EndOfRouteException();
            }

            DelayResponse result = new DelayResponse(delay, previousStopId);

            DateTimeOffset expiration = lastEvent.AddSeconds(30);
            this.logger.LogInformation($"Writing delay {delay} to {cacheKey} until {expiration}");

            this.cache.Set(cacheKey, result, expiration);

            return result;
        }
    }
}
