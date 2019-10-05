using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using PlainHttp;

namespace Bot.Services
{
    public class DelaysService : IDelaysService
    {
        private const string BASE_URL = "https://app-tpl.tndigit.it/gtlservice/trips/";

        public async Task<double?> GetDelay(string tripId)
        {
            var request = new HttpRequest(BASE_URL + tripId)
            {
                Timeout = TimeSpan.FromSeconds(1),
                Headers = new Dictionary<string, string>
                {
                    { "User-Agent", "ExpelliarbusBot (+https://t.me/expelliarbusbot)" }
                }
            };

            JsonElement delayProperty;

            try
            {
                HttpResponse response = await request.SendAsync();

                JsonDocument json = JsonDocument.Parse(response.Body);
                delayProperty = json.RootElement.GetProperty("delay");
            }
            catch
            {
                return null;
            }

            if (delayProperty.ValueKind == JsonValueKind.Number)
            {
                double delay = delayProperty.GetDouble();

                if (delay >= 0)
                {
                    return delay;
                }
            }

            return null;
        }
    }
}
