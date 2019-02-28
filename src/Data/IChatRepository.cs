using System.Threading.Tasks;

namespace Data
{
    public interface IChatRepository
    {
        Task InsertOrReplaceChat(ChatEntity chat);
    }
}
