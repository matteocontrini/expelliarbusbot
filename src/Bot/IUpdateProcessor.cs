using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Bot
{
    public interface IUpdateProcessor
    {
        Task ProcessUpdate(Update update);
    }
}
