using WorkPortalAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkPortalAPI.Repositories
{
    public class StatusRepository : IStatusRepository
    {
        private readonly WPContext _context;
        public StatusRepository(WPContext context)
        {
            this._context = context;
        }
        public async Task<Status> Create(Status status)
        {
            _context.Statuses.Add(status);
            await _context.SaveChangesAsync();
            return status;
        }

        public async Task Delete(int id)
        {
            var status = await _context.Statuses.FindAsync(id);
            _context.Statuses.Remove(status);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(string token)
        {
            var status = await _context.Statuses.FindAsync(token);
            _context.Statuses.Remove(status);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Status>> Get()
        {
            return await _context.Statuses.ToListAsync();
        }

        public async Task<Status> Get(int id)
        {
            return await _context.Statuses.FindAsync(id);
        }

        public async Task Update(Status status)
        {
            _context.Entry(status).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}
