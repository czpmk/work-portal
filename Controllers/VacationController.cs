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

        [HttpGet("DEBUG")]
        public async Task<IActionResult> Get()
        {
            return WPResponse.Success(await _vacationRepository.Get());
        }

        [HttpGet("DEBUG/{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var vacations = await _vacationRepository.Get(id);
            if (vacations != null)
                return WPResponse.Success(vacations);
            else
                return WPResponse.ArgumentDoesNotExist("id");
        }

        [HttpDelete("DEBUG/resetVacations")]
        public async Task<IActionResult> ResetChatViewReports()
        {

            await _vacationRepository.DeleteAll();
            return WPResponse.Success();
        }
    }
}
