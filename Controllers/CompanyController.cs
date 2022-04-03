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
    public class CompanyController: ControllerBase
    {
        private readonly ICompanyRepository _companyRepository;

        public CompanyController(ICompanyRepository companyRepository)
        {
            this._companyRepository = companyRepository;
        }

        [HttpGet]
        public async Task<IEnumerable<Company>> Get()
        {
            return await _companyRepository.Get();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Company>> Get(int id)
        {
            return await _companyRepository.Get(id);
        }

        [HttpGet("TestMethodToDelete2")]
        public async Task<IEnumerable<Company>> TestMethodToDelete2()
        {
            return await _companyRepository.Get();
        }
    }
}
