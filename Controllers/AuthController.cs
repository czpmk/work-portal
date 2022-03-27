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

        public AuthController(IAuthRepository authRepository)
        {
            this._authRepository = authRepository;
        }

        [HttpPost("login")]
        public async Task<Response> Login(Credentials credentials)
        {
            return await _authRepository.Login(credentials);
        }

        [HttpPost("register")]
        public async Task<Response> Register(User user)
        {
            return await _authRepository.Register(user);
        }

        [HttpPost("logout")]
        public async Task<Response> Logout(String token)
        {
            return await _authRepository.Logout(token);
        }
    }
}
