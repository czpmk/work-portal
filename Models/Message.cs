using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WorkPortalAPI.Models
{
    [Table("messages")]
    public class Message
    {
        [Column("uuid")]
        [Key]
        public string UUID { get; set; }

        [Column("chat_id")]
        public int ChatId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("timestamp")]
        public DateTime Timestamp { get; set; }

        // Max length of MS SQL string (varchar) is 8000
        [Required(ErrorMessage = "Message content required")]
        [StringLength(4096, ErrorMessage = "Invalid length", MinimumLength = 1)]
        [Column("content")]
        public string Content { get; set; }
    }
}
