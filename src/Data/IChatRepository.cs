using System.Threading.Tasks;

namespace Data
{
    public interface IChatRepository
    {
        Task DeleteChat(long id);
        Task InsertOrUpdateChat(ChatEntity chat);
    }
}
