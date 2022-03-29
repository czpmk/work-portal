using WorkPortalAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkPortalAPI.Repositories
{
    public interface IAuthRepository
    {
        Task<Response> Register(User user);
        Task<List<User>> FindUsersByEmail(string email);
        Task<List<Session>> FindSessionsByToken(string _token);
        Task<string> CreateSession(int _userID);
        Task<Session> InvalidateSession(Session session);
    }
}