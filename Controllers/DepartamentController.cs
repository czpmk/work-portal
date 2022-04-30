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
        private readonly IRoleRepository _roleRepository;
        private readonly IChatRepository _chatRepository;
        private readonly IDepartamentRepository _departamentRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IAuthRepository _authRepository;

        public DepartamentController(IRoleRepository roleRepository, IChatRepository chatRepository, IAuthRepository authRepository, ICompanyRepository companyRepository, IDepartamentRepository departamentRepository)
        {
            this._roleRepository = roleRepository;
            this._chatRepository = chatRepository;
            this._authRepository = authRepository;
            this._companyRepository = companyRepository;
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

        [HttpPost("create")]
        public async Task<IActionResult> Create(Departament departament, string token)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var user = await _authRepository.GetUserByToken(token);
            var role = await _authRepository.GetUserRoleByToken(token);
            // ADMIN OR COMPANY ADMINISTRATOR (OWNER) ONLY
            if (!user.IsAdmin && role.Type != RoleType.COMPANY_OWNER)
                return WPResponse.AccessDenied("Departament/Create");

            if (!(await _companyRepository.Exists(departament.CompanyId)))
                return WPResponse.ArgumentDoesNotExist("CompanyId");

            // Departament by the same name exists in the company
            if (await _departamentRepository.Exists(departament))
                return WPResponse.ArgumentAlreadyExists("Departament");

            var newDepartament = await _departamentRepository.Create(departament);

            // CREATE DEPARTAMENT CHAT
            var chat = new Chat()
            {
                CompanyId = newDepartament.CompanyId,
                DepartamentId = newDepartament.Id
            };
            var newChat = await _chatRepository.Create(chat);

            return WPResponse.Custom(newDepartament);
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> Delete(Departament departament, string token)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var user = await _authRepository.GetUserByToken(token);
            var role = await _authRepository.GetUserRoleByToken(token);
            // ADMIN OR COMPANY ADMINISTRATOR (OWNER) ONLY
            if (!user.IsAdmin && role.Type != RoleType.COMPANY_OWNER)
                return WPResponse.AccessDenied("Departament/Create");

            if (!(await _departamentRepository.Exists(departament.Id)))
                return WPResponse.ArgumentDoesNotExist("DepartamentId");

            // REMOVE DEPARTAMENT CHAT
            var departamentChat = await _chatRepository.GetDepartamentChat(departament.CompanyId, departament.Id);
            if (departamentChat != null)
                await _chatRepository.Delete(departamentChat.Id);

            // REMOVE DEPARTAMENTS
            await _departamentRepository.Delete(departament.Id);

            return WPResponse.Custom();
        }
    }
}
