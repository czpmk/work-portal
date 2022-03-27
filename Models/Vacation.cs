using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace work_portal.Models
{
    public enum VacationType // TODO: Add correct vacation types
    {
        SOME_TYPE = 1,
        ANOTHER_TYPE = 2
    }

    public enum VacationRequestState
    {
        PENDING = 1,
        ACCEPTED = 2,
        REJECTED = 3
    }

    [Table("vacations")]
    public class Vacation
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("company_id")]
        public int CompanyId { get; set; }

        [Column("type")]
        public VacationType Type { get; set; }

        [Column("state")]
        public VacationRequestState State { get; set; }

        [Column("state_change_time")]
        public DateTime ModificationTime { get; set; }

        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Column("end_date")]
        public DateTime EndDate { get; set; }
    }
}
