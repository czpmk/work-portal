using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WorkPortalAPI.Models
{
    [Table("chats")]
    public class Chat
    {
        [Column("id")]
        [SwaggerSchema(ReadOnly = true)]
        public int Id { get; set; }

        [Column("company_id")]
        public int? CompanyId { get; set; }

        [Column("departament_id")]
        public int? DepartamentId { get; set; }

        [Column("user_1_id")]
        public int? FirstUserId { get; set; }

        [Column("user_2_id")]
        public int? SecondUserId { get; set; }
    }
}
