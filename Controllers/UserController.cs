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
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthRepository _authRepository;

        public UserController(IAuthRepository authRepository, IUserRepository userRepository)
        {
            this._userRepository = userRepository;
            this._authRepository = authRepository;
        }

        [HttpGet("DEBUG")]
        public async Task<IActionResult> Get()
        {
            return WPResponse.Success(await _userRepository.Get());
        }

        [HttpGet("DEBUG/{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var user = await _userRepository.Get(id);
            if (user == null)
                return WPResponse.ArgumentDoesNotExist("id");
            else
                return WPResponse.Success(user);
        }

        [HttpGet("DEBUG/myUserInfo")]
        public async Task<IActionResult> GetMyUserInfo(string token)
        {
            var user = await _authRepository.GetUserByToken(token);
            return WPResponse.Success(user);
        }

        [HttpGet("DEBUG/myRoleInfo")]
        public async Task<IActionResult> GetMyRoleInfo(string token)
        {
            var role = await _authRepository.GetUserRoleByToken(token);
            return WPResponse.Success(role);
        }

        //[HttpPut("create")]
        //public async Task<IActionResult> CreateUser(User user, string token)
        //{
        //    return WPResponse.Success(user);
        //}
    }
}
