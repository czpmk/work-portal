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
            string _token = Utils.NewUUID();
            Session session = new();
            session.UserId = _userID;
            session.Token = _token;
            session.ExpiryTime = DateTime.Now.AddHours(12);

            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();

            return _token;
        }

        //TODO: Move to UserRepository?
        public async Task<List<User>> GetUsersByEmail(string _email)
        {
            var query = from user in _context.Users where user.Email.Equals(_email) select user;
            return await query.ToListAsync();
        }

        public async Task<List<Session>> GetSessionsByToken(string _token)
        {
            var query = from session in _context.Sessions where session.Token.Equals(_token) select session;
            return await query.ToListAsync();
        }

        public async Task<Session> TerminateSession(Session session)
        {
            _context.Sessions.Remove(session);
            await _context.SaveChangesAsync();
            return session;
        }

        public async Task<string> TerminateSession(string token)
        {
            var sessions = await GetSessionsByToken(token);
            foreach (var s in sessions)
                await TerminateSession(s);
            return token;
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

            // multiple sessions with the same token, or session expired
            if (!sessions.Any() || sessions.Count() != 1 || sessions.First().ExpiryTime < DateTime.Now)
            {
                return false;
            }
            // session valid OK
            else
            {
                return true;
            }
        }

        public async Task<User> GetUserByToken(string token)
        {
            var session = (await GetSessionsByToken(token)).FirstOrDefault();
            if (session == null)
                return null;
            return await _context.Users.FindAsync(session.UserId);
        }

        public async Task<Role> GetUserRoleByToken(string token)
        {
            var user = await GetUserByToken(token);
            return await _context.Roles.Where(r => r.UserId == user.Id).FirstOrDefaultAsync();
        }

        public async Task<Boolean> CheckAccess(int requestingUserId, int targetUserId,
            List<RoleType> eligibleRoles)
        {
            var requestingUser = await _context.Users.FindAsync(requestingUserId);
            var requestingUsersRole = await _context.Roles.Where(r => r.UserId == requestingUser.Id).FirstOrDefaultAsync();

            var targetUser = await _context.Users.FindAsync(targetUserId);
            var targetUsersRole = await _context.Roles.Where(r => r.UserId == targetUser.Id).FirstOrDefaultAsync();

            if (requestingUserId == targetUserId)
                return true;

            else if (eligibleRoles.Contains(RoleType.HEAD_OF_DEPARTMENT) &&
                requestingUsersRole.Type == RoleType.HEAD_OF_DEPARTMENT &&
                requestingUsersRole.DepartmentId == targetUsersRole.DepartmentId)
                return true;

            else if (eligibleRoles.Contains(RoleType.COMPANY_OWNER) &&
                requestingUsersRole.Type == RoleType.COMPANY_OWNER &&
                requestingUsersRole.CompanyId == targetUsersRole.CompanyId)
                return true;

            else if (requestingUser.IsAdmin)
                return true;

            else
                return false;
        }
    }
}
