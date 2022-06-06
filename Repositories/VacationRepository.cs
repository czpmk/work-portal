using WorkPortalAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkPortalAPI.Repositories
{
    public class VacationRepository : IVacationRepository
    {
        private readonly WPContext _context;
        public VacationRepository(WPContext context)
        {
            this._context = context;
        }
        public async Task<Vacation> Create(Vacation vacation)
        {
            _context.Vacations.Add(vacation);
            await _context.SaveChangesAsync();
            return vacation;
        }

        public async Task Delete(int id)
        {
            var vacation = await _context. Vacations.FindAsync(id);
            _context.Vacations.Remove(vacation);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Vacation>> Get()
        {
            return await _context.Vacations.ToListAsync();
        }

        public async Task<Vacation> Get(int id)
        {
            return await _context.Vacations.FindAsync(id);
        }

        public async Task<List<Vacation>> GetByUserId(int userId)
        {
            return await _context.Vacations.Where(v => v.UserId == userId).ToListAsync();
        }

        public async Task<List<Vacation>> GetByCompanyId(int companyId)
        {
            var vacationIds = await _context.Vacations.Join(_context.Roles, v => v.UserId, r => r.UserId,
                                                                            (v, r) => new
                                                                            {
                                                                                Id = v.Id,
                                                                                CompanyId = r.CompanyId,
                                                                            }
                                                                        ).Where(vr => vr.CompanyId == companyId)
                                                                        .Select(vr => vr.Id)
                                                                        .ToListAsync();

            return await _context.Vacations.Where(v => vacationIds.Contains(v.Id)).ToListAsync();
        }

        public async Task<List<Vacation>> GetByDepartmentId(int companyId, int departmentId)
        {
            var vacationIds = await _context.Vacations.Join(_context.Roles, v => v.UserId, r => r.UserId,
                                                                            (v, r) => new
                                                                            {
                                                                                Id = v.Id,
                                                                                CompanyId = r.CompanyId,
                                                                                DepartmentId = r.DepartmentId,
                                                                            }
                                                                        ).Where(vr => vr.DepartmentId == departmentId &&
                                                                                      vr.CompanyId == companyId)
                                                                        .Select(vr => vr.Id)
                                                                        .ToListAsync();

            return await _context.Vacations.Where(v => vacationIds.Contains(v.Id)).ToListAsync();
        }

        public async Task Update(Vacation vacation)
        {
            vacation.ModificationTime = DateTime.Now;
            _context.Entry(vacation).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAll()
        {
            await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE dbo.vacations");
        }
    }
}
