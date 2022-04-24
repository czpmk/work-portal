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
        public DbSet<Company> Companies { get; set; }
        public DbSet<Departament> Departaments { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Vacation> Vacations { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Chat> Chats { get; set; }
    }
}