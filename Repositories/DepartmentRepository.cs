using WorkPortalAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkPortalAPI.Repositories
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly WPContext _context;
        public DepartmentRepository(WPContext context)
        {
            this._context = context;
        }
        public async Task<Department> Create(Department departament)
        {
            _context.Departments.Add(departament);
            await _context.SaveChangesAsync();
            return departament;
        }

        public async Task Delete(int id)
        {
            var departament = await _context.Departments.FindAsync(id);
            _context.Departments.Remove(departament);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Department>> Get()
        {
            return await _context.Departments.ToListAsync();
        }

        public async Task<Department> Get(int id)
        {
            return await _context.Departments.FindAsync(id);
        }

        // TODO: remove ?
        public async Task<IEnumerable<Department>> GetByCompanyId(int companyId)
        {
            var gowno = from d in _context.Departments where d.CompanyId == companyId select d;
            return await gowno.ToListAsync();
        }

        public async Task Update(Department departament)
        {
            _context.Entry(departament).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<Boolean> Exists(int id)
        {
            return await _context.Departments.Where(d => d.Id == id).AnyAsync();
        }

        public async Task<Boolean> Exists(Department departament)
        {
            return await _context.Departments.
                Where(d => d.CompanyId == departament.CompanyId && d.Name == departament.Name).AnyAsync();
        }

        public async Task<User> GetOwner(Department departament)
        {
            var role = await _context.Roles.Where(r => r.Type == RoleType.HEAD_OF_DEPARTMENT &&
                                                   r.DepartmentId == departament.Id).FirstOrDefaultAsync();
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
                role.Type = RoleType.HEAD_OF_DEPARTMENT;
                role.DepartmentId = departamentId;
                _context.Entry(role).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            return user;
        }

        public async Task DeleteAll()
        {
            await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE dbo.departaments");
        }
    }
}
