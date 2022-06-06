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
    public class RoleController
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IAuthRepository _authRepository;

        public RoleController(IRoleRepository roleRepository, IAuthRepository authRepository)
        {
            this._roleRepository = roleRepository;
            this._authRepository = authRepository;
        }

        [HttpGet()]
        public async Task<IActionResult> Get(string token)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var requestingUser = await _authRepository.GetUserByToken(token);
            var requestingUsersRole = await _roleRepository.GetByUserId(requestingUser.Id);

            return WPResponse.Success(
                new Dictionary<string, object> {
                    { "role", requestingUsersRole.Type },
                    { "isAdmin", requestingUser.IsAdmin} }
                );
        }

        [HttpGet("DEBUG/{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var role = await _roleRepository.Get(id);
            if (role != null)
                return WPResponse.Success(role);
            else
                return WPResponse.ArgumentDoesNotExist("id");
        }

        [HttpPut("DEBUG/create")]
        public async Task<IActionResult> Create(Role role)
        {

            if (role != null)
            {
                await _roleRepository.Create(role);
                return WPResponse.Success(role);
            }

            else
                return WPResponse.ArgumentDoesNotExist("id");
        }

        [HttpDelete("DEBUG/resetRoles")]
        public async Task<IActionResult> ResetChatViewReports()
        {

            await _roleRepository.DeleteAll();
            return WPResponse.Success();
        }
    }
}
