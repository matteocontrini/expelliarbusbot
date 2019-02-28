using SQLite;
using System;

namespace Data
{
    [Table("chats")]
    public class ChatEntity
    {
        [Column("chat_id")]
        [PrimaryKey]
        public long ChatId { get; set; }

        [Column("type")]
        public string Type { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("username")]
        public string Username { get; set; }

        [Column("first_name")]
        public string FirstName { get; set; }

        [Column("last_name")]
        public string LastName { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}
