using WorkPortalAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkPortalAPI.Repositories
{
    public interface IAuthRepository
    {
        Task<Response> Login(Credentials credentials);
        Task<Response> Register(User user);
        Task<Response> Logout(String token);
    }
}