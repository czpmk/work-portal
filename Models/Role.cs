using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace work_portal.Models
{
    public enum RoleType {
        User = 1,
        Administrator = 2
    }

    [Table("users_roles")]
    public class Role
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("user_id")]
        public int UserId { get; set; }
        [Column("company_id")]
        public int CompanyId { get; set; }
        [Column("departament_id")]
        public int DepartamentId { get; set; }
        [Column("type")] // TODO: change type to int
        public RoleType Type { get; set; }
    }
}
