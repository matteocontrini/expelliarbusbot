using System;
using System.Linq;
using System.Threading.Tasks;
using SQLite;

namespace Data
{
    public class ChatRepository : IChatRepository
    {
        private readonly SQLiteAsyncConnection connection;

        public ChatRepository(ISQLiteFactory factory)
        {
            this.connection = factory.GetConnection("bot");
        }

        public async Task InsertOrUpdateChat(ChatEntity chat)
        {
            ChatEntity existing = (await this.connection.QueryAsync<ChatEntity>("SELECT * FROM chats WHERE chat_id = ?", chat.ChatId))
                .SingleOrDefault();

            if (existing == null)
            {
                chat.UpdatedAt = DateTime.UtcNow;
                chat.CreatedAt = DateTime.UtcNow;
                await this.connection.InsertAsync(chat);
            }
            else
            {
                chat.CreatedAt = existing.CreatedAt;
                chat.UpdatedAt = DateTime.UtcNow;
                await this.connection.UpdateAsync(chat);
            }
        }

        public Task DeleteChat(long id)
        {
            return this.connection.DeleteAsync<ChatEntity>(id);
        }
    }
}
