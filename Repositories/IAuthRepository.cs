using WorkPortalAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkPortalAPI.Repositories
{
    public interface IAuthRepository
    {
        Task<User> CreateUser(User user);
        Task<List<User>> FindUsersByEmail(string email);
        Task<List<Session>> FindSessionsByToken(string _token);
        Task<User> FindUserByToken(string token);
        Task<string> CreateSession(int _userID);
        Task<Session> TerminateSession(Session session);
        Task<string> TerminateSession(string token);
        Task<Boolean> SessionValid(string token);
    }
}