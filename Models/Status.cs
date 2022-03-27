using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WorkPortalAPI.Models
{
    public enum StatusType { 
        Work = 1,
        Break = 2,
        OutOfOffice = 3
    }


    [Table("statuses")]
    public class Status
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("type")]
        public StatusType Type { get; set; }

        // TODO: setter: set(x: DateTime) => ToString("yyyy-MM-dd HH:mm:ss.fff") if required 
        [Column("datetime")]
        public DateTime Timestamp { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }
    }
}
