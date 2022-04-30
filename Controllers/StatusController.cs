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

        [HttpGet("DEBUG")]
        public async Task<IActionResult> Get()
        {
            return WPResponse.Success(await _statusRepository.Get());
        }

        [HttpGet("DEBUG/{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var status = await _statusRepository.Get(id);
            if (status != null)
                return WPResponse.Success(status);
            else
                return WPResponse.ArgumentDoesNotExist("id");
        }
    }
}
