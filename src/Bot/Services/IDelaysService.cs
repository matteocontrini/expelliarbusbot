using System.Threading.Tasks;

namespace Bot.Services
{
    public interface IDelaysService
    {
        Task<DelayResponse> GetDelay(string tripId);
    }
}
