using System.Threading.Tasks;

namespace Data
{
    public interface IChatRepository
    {
        Task InsertOrReplaceChat(ChatEntity chat);
        Task DeleteChat(long id);
    }
}
