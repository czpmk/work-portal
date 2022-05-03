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

        public RoleController(IRoleRepository roleRepository)
        {
            this._roleRepository = roleRepository;
        }

        [HttpGet("DEBUG")]
        public async Task<IActionResult> Get()
        {
            return WPResponse.Success(await _roleRepository.Get());
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

        [HttpGet("DEBUG/create")]
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
    }
}
