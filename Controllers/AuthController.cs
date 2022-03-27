using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkPortalAPI.Repositories;
using WorkPortalAPI.Models;

namespace WorkPortalAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;
        private readonly IUserRepository _userRepository;

        public AuthController(IAuthRepository authRepository, IUserRepository userRepository)
        {
            this._authRepository = authRepository;
            this._userRepository = userRepository;
        }

        [HttpPost("login")]
        public async Task<Response> Login(Credentials credentials)
        {
            return await _authRepository.Login(credentials);
        }

        [HttpPost("register")]
        public async Task<Response> Register(User user)
        {
            // TODO:
            // data validation
            // generate salt
            // hash the password
            // add a proper response
            var r = new Response();
            await _userRepository.Create(user);
            return r;
        }

        [HttpPost("logout")]
        public async Task<Response> Logout(String token)
        {
            return await _authRepository.Logout(token);
        }
    }
}
