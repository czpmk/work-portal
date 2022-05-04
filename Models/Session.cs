using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WorkPortalAPI.Models
{
    public class Session
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("token")]
        public string Token { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("expiry_time")]
        public DateTime ExpiryTime { get; set; }
    }
}
