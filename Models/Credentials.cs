using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WorkPortalAPI.Models
{
    public class Credentials
    {
        public string Email { get; set; }

        public string PasswordHash  { get; set; }
    }
}
