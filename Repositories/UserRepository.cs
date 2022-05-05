using WorkPortalAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkPortalAPI.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly WPContext _context;
        public UserRepository(WPContext context)
        {
            this._context = context;
        }
        public async Task<User> Create(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task Delete(int id)
        {
            var chatViewReports = await _context.ChatViewReports.Where(cvr => cvr.UserId == id).ToListAsync();
            foreach(var cvr in chatViewReports)
            {
                _context.ChatViewReports.Remove(cvr);
            }

            var chats = await _context.Chats.Where(c => c.FirstUserId == id || c.SecondUserId == id).ToListAsync();
            foreach (var c in chats)
            {
                _context.Chats.Remove(c);
            }

            var messages = await _context.Messages.Where(m => m.UserId == id).ToListAsync();
            foreach (var m in messages)
            {
                _context.Messages.Remove(m);
            }

            var role = await _context.Roles.Where(r => r.UserId == id).FirstAsync();
            _context.Roles.Remove(role);

            var sessions = await _context.Sessions.Where(s => s.UserId == id).ToListAsync();
            foreach (var s in sessions)
            {
                _context.Sessions.Remove(s);
            }

            var statuses = await _context.Statuses.Where(st => st.UserId == id).ToListAsync();
            foreach (var st in statuses)
            {
                _context.Statuses.Remove(st);
            }

            var vacations = await _context.Vacations.Where(v => v.UserId == id).ToListAsync();
            foreach (var v in vacations)
            {
                _context.Vacations.Remove(v);
            }

            var user = await _context.Users.FindAsync(id);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<User>> Get()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User> Get(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task Update(User user)
        {
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<Boolean> Exists(int id)
        {
            return await _context.Users.Where(u => u.Id == id).AnyAsync();
        }

        public async Task<Boolean> Exists(string email)
        {
            return await _context.Users.Where(u => u.Email == email).AnyAsync();
        }

    }
}
