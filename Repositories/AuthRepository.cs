using WorkPortalAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkPortalAPI.Repositories
{
    public class AuthRepository : IAuthRepository
    {

        private readonly WPContext _context;
        public AuthRepository(WPContext context)
        {
            this._context = context;
        }

        //returns token string of created session
        public async Task<string> CreateSession(int _userID)
        {
            string _token = Guid.NewGuid().ToString().Replace("-", "");
            Session session = new();
            session.UserId = _userID;
            session.Token = _token;
            session.ExpiryTime = DateTime.Now.AddHours(12);

            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();

            return _token;
        }

        //TODO: Move to UserRepository?
        public async Task<List<User>> FindUsersByEmail(string _email)
        {
            var query = from user in _context.Users where user.Email.Equals(_email) select user;
            return await query.ToListAsync();
        }

        public async Task<List<Session>> FindSessionsByToken(string _token)
        {
            var query = from session in _context.Sessions where session.Token.Equals(_token) select session;
            return await query.ToListAsync();
        }

        public async Task<Session> InvalidateSession(Session session)
        {
            _context.Sessions.Remove(session);
            await _context.SaveChangesAsync();
            return session;
        }

        public async Task<User> CreateUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<Boolean> SessionValid(string token)
        {
            var sessions = await _context.Sessions.Where(s => s.Token == token).ToListAsync();
            if (sessions.Any())
            {
                // multiple sessions with the same token, or session expired
                if (sessions.Count() != 1 || sessions.First().ExpiryTime < DateTime.Now)
                {
                    _context.Remove(token);
                    return false;
                }
                // session valid OK
                else
                {
                    return true;
                }
            }
            // session with the token given not found
            else
            {
                return false;
            }
        }
    }
}
