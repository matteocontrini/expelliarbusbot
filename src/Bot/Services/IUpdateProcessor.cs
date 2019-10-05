using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Bot.Services
{
    public interface IUpdateProcessor
    {
        Task ProcessUpdate(Update update);
    }
}
