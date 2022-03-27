using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace work_portal.Models
{
    public enum StatusType { 
        Work = 1,
        Break = 2,
        OutOfOffice = 3
    }


    [Table("status_history")]
    public class Status
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("status_id")]  // TODO: int Type in DB, enum StatusId here; change name to Type
        public int Type { get; set; }
        // TODO: setter: set(x: DateTime) => ToString("yyyy-MM-dd HH:mm:ss.fff") if required 
        [Column("datetime")]
        public DateTime Timestamp { get; set; }
        [Column("user_id")]
        public int UserId { get; set; }
        [Column("company_id")]
        public int CompanyId { get; set; }
    }
}
