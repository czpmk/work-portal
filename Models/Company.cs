using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WorkPortalAPI.Models
{
    [Table("companies")]
    public class Company
    {
        [Column("id")]
        [SwaggerSchema(ReadOnly = true)]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }
    }
}
