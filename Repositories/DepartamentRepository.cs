using WorkPortalAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkPortalAPI.Repositories
{
    public class DepartamentRepository : IDepartamentRepository
    {
        private readonly WPContext _context;
        public DepartamentRepository(WPContext context)
        {
            this._context = context;
        }
        public async Task<Departament> Create(Departament departament)
        {
            _context.Departaments.Add(departament);
            await _context.SaveChangesAsync();
            return departament;
        }

        public async Task Delete(int id)
        {
            var departament = await _context.Departaments.FindAsync(id);
            _context.Departaments.Remove(departament);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Departament>> Get()
        {
            return await _context.Departaments.ToListAsync();
        }

        public async Task<Departament> Get(int id)
        {
            return await _context.Departaments.FindAsync(id);
        }

        // TODO: remove ?
        public async Task<IEnumerable<Departament>> GetByCompanyId(int companyId)
        {
            var gowno = from d in _context.Departaments where d.CompanyId == companyId select d;
            return await gowno.ToListAsync();
        }

        public async Task Update(Departament departament)
        {
            _context.Entry(departament).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<Boolean> Exists(int id)
        {
            return await _context.Departaments.Where(d => d.Id == id).AnyAsync();
        }

        public async Task<Boolean> Exists(Departament departament)
        {
            return await _context.Departaments.
                Where(d => d.CompanyId == departament.CompanyId && d.Name == departament.Name).AnyAsync();
        }

        public async Task<User> GetOwner(Departament departament)
        {
            var role = await _context.Roles.Where(r => r.Type == RoleType.HEAD_OF_DEPARTAMENT &&
                                                   r.DepartamentId == departament.Id).FirstOrDefaultAsync();
            if (role == null)
                return null;
            else
                return await _context.Users.FindAsync(role.UserId);
        }

        public async Task<User> RetractOwnership(User user)
        {
            var role = await _context.Roles.Where(r => r.UserId == user.Id).FirstOrDefaultAsync();
            if (role != null)
            {
                role.Type = RoleType.USER;
                _context.Entry(role).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            return user;
        }

        public async Task<User> GrantOwnership(User user, RoleType type, int departamentId)
        {
            var role = await _context.Roles.Where(r => r.UserId == user.Id).FirstOrDefaultAsync();
            if (role != null)
            {
                role.Type = RoleType.HEAD_OF_DEPARTAMENT;
                role.DepartamentId = departamentId;
                _context.Entry(role).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            return user;
        }
    }
}
