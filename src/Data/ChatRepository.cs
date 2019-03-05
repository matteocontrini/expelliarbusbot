using SQLite;
using System.Threading.Tasks;

namespace Data
{
    public class ChatRepository : IChatRepository
    {
        private readonly SQLiteAsyncConnection connection;

        public ChatRepository(ISQLiteFactory factory)
        {
            this.connection = factory.GetConnection("bot");
        }

        public Task InsertOrReplaceChat(ChatEntity chat)
        {
            return this.connection.InsertOrReplaceAsync(chat);
        }

        public Task DeleteChat(long id)
        {
            return this.connection.DeleteAsync<ChatEntity>(id);
        }
    }
}
