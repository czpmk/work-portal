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
    public class DepartamentController : ControllerBase
    {
        private readonly IDepartamentRepository _departamentRepository;

        public DepartamentController(IDepartamentRepository departamentRepository)
        {
            this._departamentRepository = departamentRepository;
        }

        [HttpGet]
        public async Task<IEnumerable<Departament>> Get()
        {
            return await _departamentRepository.Get();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Departament>> Get(int id)
        {
            return await _departamentRepository.Get(id);
        }

        [HttpGet("byCompanyId/{companyId}")]
        public async Task<IEnumerable<Departament>> GetByCompanyId(int companyId)
        {
            //var cc = DependencyResolver.Current.GetService<CompanyController>();
            return await _departamentRepository.GetByCompanyId(companyId);
        }
    }
}
