using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace work_portal.Models
{
    [Table("departaments")]
    public class Departament
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("company_id")]
        public int CompanyId { get; set; }
        [Column("name")]
        public string Name { get; set; }
    }
}
