using WorkPortalAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkPortalAPI.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly WPContext _context;
        public RoleRepository(WPContext context)
        {
            this._context = context;
        }
        public async Task<Role> Create(Role role)
        {
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
            return role;
        }

        public async Task Delete(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Role>> Get()
        {
            return await _context.Roles.ToListAsync();
        }

        public async Task<Role> Get(int id)
        {
            return await _context.Roles.FindAsync(id);
        }

        public async Task<Role> GetByUserId(int userId)
        {
            return await _context.Roles.Where(r => r.UserId == userId).FirstOrDefaultAsync();
        }

        public async Task Update(Role role)
        {
            _context.Entry(role).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAll()
        {
            await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE dbo.roles");
        }
    }
}
