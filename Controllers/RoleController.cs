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

        [HttpGet]
        public async Task<IEnumerable<Role>> Get()
        {
            return await _roleRepository.Get();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Role>> Get(int id)
        {
            return await _roleRepository.Get(id);
        }
    }
}
