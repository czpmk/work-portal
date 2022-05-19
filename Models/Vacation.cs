using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WorkPortalAPI.Models
{
    public enum VacationType
    {
        QUARANTINE = 1,
        SICK_LEAVE = 2,
        UNPAID_LEAVE = 3,
        PARENTIAL_LEAVE = 4,
        MATERNITY_LEAVE = 5,
        VACATION_LEAVE = 6,
        OCCASIONAL_LEAVE = 7,
        VACATION_ON_DEMAND_LEAVE = 8,
        BLOOD_DONATION_LEAVE = 9
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
        [SwaggerSchema(ReadOnly = true)]
        [Column("id")]
        public int Id { get; set; }

        [SwaggerSchema(ReadOnly = true)]
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("type")]
        public VacationType Type { get; set; }

        [SwaggerSchema(ReadOnly = true)]
        [Column("state")]
        public VacationRequestState State { get; set; }

        [SwaggerSchema(ReadOnly = true)]
        [Column("modification_time")]
        public DateTime ModificationTime { get; set; }

        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Column("end_date")]
        public DateTime EndDate { get; set; }
    }
}
