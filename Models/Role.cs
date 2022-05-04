using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WorkPortalAPI.Models
{
    public enum RoleType {
        COMPANY_OWNER = 1,
        HEAD_OF_DEPARTAMENT = 2,
        USER = 3
    }

    [Table("roles")]
    public class Role
    {
        [Column("id")]
        [SwaggerSchema(ReadOnly = true)]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("company_id")]
        public int CompanyId { get; set; }

        [Column("departament_id")]
        public int DepartamentId { get; set; }

        [Column("type")]
        public RoleType Type { get; set; }
    }
}
