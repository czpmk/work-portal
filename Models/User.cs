using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;


namespace WorkPortalAPI.Models
{
    [Table("users")]
    public class User
    {
        [Required(AllowEmptyStrings = true)]
        [Column("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "First name required")]
        [StringLength(64, ErrorMessage = "Invalid length", MinimumLength = 2)]
        [Column("first_name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Surname required")]
        [StringLength(64, ErrorMessage = "Invalid length", MinimumLength = 2)]
        [Column("sur_name")]
        public string Surname { get; set; }

        [Required(ErrorMessage = "Email required")]
        [StringLength(64, ErrorMessage = "Invalid length", MinimumLength = 5)]
        [Column("email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password required")]
        [StringLength(64, ErrorMessage = "Invalid length", MinimumLength = 2)]
        [Column("password")]
        public string Password { get; set; }

        [Column("profile_pic")]
        public string ProfilePicture { get; set; }

        [Required(AllowEmptyStrings = true)]
        [Column("is_admin")]
        public bool IsAdmin { get; set; }

        [Column("salt")]
        public string Salt { get; set; }

        [Required(ErrorMessage = "Language required")]
        [StringLength(32, ErrorMessage = "Invalid length", MinimumLength = 2)]
        [Column("language")]
        public string Language { get; set; }
    }
}
