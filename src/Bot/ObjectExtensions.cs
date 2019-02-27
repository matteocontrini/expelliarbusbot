using Newtonsoft.Json;

namespace Bot
{
    public static class ObjectExtensions
    {
        public static string ToJson(this object update)
        {
            return JsonConvert.SerializeObject(update);
        }
    }
}
