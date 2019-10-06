using System.Threading.Tasks;

namespace Bot.Services
{
    public interface IDelaysService
    {
        Task<double> GetDelay(string tripId);
    }
}
