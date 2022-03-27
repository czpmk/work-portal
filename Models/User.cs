using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WorkPortalAPI.Models
{
    [Table("users")]
    public class User
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("first_name")]
        public string FirstName { get; set; }
        [Column("sur_name")]
        public string Surname { get; set; }
        [Column("email")]
        public string Email { get; set; }
        [Column("password")]
        public string Password { get; set; }
        [Column("profile_pic")]
        public string ProfilePicture { get; set; }
        [Column("privilege_level")]
        public string PrivilegeLevel { get; set; }
        [Column("company_id")]
        public int CompanyId { get; set; }
        [Column("salt")]
        public string Salt { get; set; }
        [Column("language")]
        public string Language { get; set; }
    }
}
