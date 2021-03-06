using Swashbuckle.AspNetCore.Annotations;
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
        [SwaggerSchema(ReadOnly = true)]
        public int Id { get; set; }

        [Column("type")]
        public StatusType Type { get; set; }

        [Column("timestamp")]
        public DateTime Timestamp { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }
    }
}
