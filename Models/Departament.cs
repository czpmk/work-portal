using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace WorkPortalAPI.Models
{
    [Table("departaments")]
    public class Departament
    {
        [Column("id")]
        [SwaggerSchema(ReadOnly = true)]
        public int Id { get; set; }

        [Column("company_id")]
        public int CompanyId { get; set; }

        [Column("name")]
        public string Name { get; set; }

    }
}
