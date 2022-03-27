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
    public class StatusController
    {
        private readonly IStatusRepository _statusRepository;

        public StatusController(IStatusRepository statusRepository)
        {
            this._statusRepository = statusRepository;
        }

        [HttpGet]
        public async Task<IEnumerable<Status>> Get()
        {
            return await _statusRepository.Get();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Status>> Get(int id)
        {
            return await _statusRepository.Get(id);
        }
    }
}
