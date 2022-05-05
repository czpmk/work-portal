using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;


namespace WorkPortalAPI.Models
{
    [Table("users")]
    public class UserEdition
    {

        [StringLength(64, ErrorMessage = "Invalid length", MinimumLength = 2)]
        [Column("first_name")]
        public string FirstName { get; set; }

        [StringLength(64, ErrorMessage = "Invalid length", MinimumLength = 2)]
        [Column("sur_name")]
        public string Surname { get; set; }

        [StringLength(64, ErrorMessage = "Invalid length", MinimumLength = 5)]
        [Column("email")]
        public string Email { get; set; }

        [DefaultValue(false)]
        [Column("is_admin")]
        public bool IsAdmin { get; set; }

        [StringLength(32, ErrorMessage = "Invalid length", MinimumLength = 2)]
        [Column("language")]
        public string Language { get; set; }
    }
}
