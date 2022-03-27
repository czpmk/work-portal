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
            _context. Vacations.Add(vacation);
            await _context.SaveChangesAsync();
            return vacation;
        }

        public async Task Delete(int id)
        {
            var vacation = await _context. Vacations.FindAsync(id);
            _context. Vacations.Remove(vacation);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Vacation>> Get()
        {
            return await _context. Vacations.ToListAsync();
        }

        public async Task<Vacation> Get(int id)
        {
            return await _context. Vacations.FindAsync(id);
        }

        public async Task Update(Vacation vacation)
        {
            _context.Entry(vacation).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}
