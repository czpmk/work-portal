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
    public class VacationController : ControllerBase
    {
        private readonly IVacationRepository _vacationRepository;

        public VacationController(IVacationRepository vacationRepository)
        {
            this._vacationRepository = vacationRepository;
        }

        [HttpGet]
        public async Task<IEnumerable<Vacation>> Get()
        {
            return await _vacationRepository.Get();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Vacation>> Get(int id)
        {
            return await _vacationRepository.Get(id);
        }
    }
}
