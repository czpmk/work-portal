using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WorkPortalAPI.Models
{
    public enum ChatViewStatus
    {
        UP_TO_DATE = 0,
        NEW_MESSAGES_AWAITING = 1
    }

    [Table("chat_view_reports")]
    public class ChatViewReport
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("chat_id")]
        public int ChatId { get; set; }

        [Column("message_id")]
        public string? MessageUUID { get; set; }
    }
}
