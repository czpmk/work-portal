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

        public Task<Response> Login(Credentials credentials)
        {
            throw new NotImplementedException();
            //var dbResponse = from user in _context.Users where user.Email.Equals(credentials.Email) select user;
            //if(dbResponse.Count())
        }

        public Task<Response> Logout(string token)
        {
            throw new NotImplementedException();
        }

        public Task<Response> Register(User user)
        {
            throw new NotImplementedException();
        }
    }
}
