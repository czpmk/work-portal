using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkPortalAPI.Models;

namespace WorkPortalAPI.Models
{
    public class WPContext : DbContext
    {
        public WPContext(DbContextOptions<WPContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }
        public DbSet<User> Users { get; set; }
    }
}