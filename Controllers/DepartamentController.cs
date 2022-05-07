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
        private readonly IUserRepository _userRepository;

        public DepartamentController(IUserRepository userRepository, IRoleRepository roleRepository, IChatRepository chatRepository, IAuthRepository authRepository, ICompanyRepository companyRepository, IDepartamentRepository departamentRepository)
        {
            this._roleRepository = roleRepository;
            this._chatRepository = chatRepository;
            this._authRepository = authRepository;
            this._companyRepository = companyRepository;
            this._departamentRepository = departamentRepository;
            this._userRepository = userRepository;
        }

        [HttpGet("DEBUG")]
        public async Task<IActionResult> Get()
        {
            return WPResponse.Success(await _departamentRepository.Get());
        }

        [HttpGet("DEBUG/{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var departament = await _departamentRepository.Get(id);
            if (departament == null)
                return WPResponse.ArgumentDoesNotExist("id");
            else
                return WPResponse.Success(departament);
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

            return WPResponse.Success(newDepartament);
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> Delete(int departamentId, string token)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var user = await _authRepository.GetUserByToken(token);
            var role = await _authRepository.GetUserRoleByToken(token);
            // ADMIN OR COMPANY ADMINISTRATOR (OWNER) ONLY
            if (!user.IsAdmin && role.Type != RoleType.COMPANY_OWNER)
                return WPResponse.AccessDenied("Departament/Create");

            if (!(await _departamentRepository.Exists(departamentId)))
                return WPResponse.ArgumentDoesNotExist("DepartamentId");

            var departament = await _departamentRepository.Get(departamentId);

            // REMOVE DEPARTAMENT CHAT
            var departamentChat = await _chatRepository.GetDepartamentChat(departament.CompanyId, departament.Id);
            if (departamentChat != null)
                await _chatRepository.Delete(departamentChat.Id);

            // REMOVE DEPARTAMENTS
            await _departamentRepository.Delete(departament.Id);

            return WPResponse.Success();
        }

        [HttpPut("SetHeadOfDepartament")]
        public async Task<IActionResult> SetHeadOfDepartament(int departamentId, int userId, string token)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var user = await _authRepository.GetUserByToken(token);
            if (!user.IsAdmin)
                return WPResponse.AccessDenied("Departament");

            if (!(await _departamentRepository.Exists(departamentId)))
                return WPResponse.ArgumentDoesNotExist("departamentId");

            var departament = await _departamentRepository.Get(departamentId);
            var newHead = await _userRepository.Get(userId);

            if (!(await _userRepository.Exists(newHead.Id)))
                return WPResponse.ArgumentDoesNotExist("newOwnerId");

            // get old owner
            var oldHead = await _departamentRepository.GetOwner(departament);
            if (oldHead != null)
            {
                await _departamentRepository.RetractOwnership(oldHead);
            }

            await _departamentRepository.GrantOwnership(newHead, RoleType.HEAD_OF_DEPARTAMENT, departament.Id);
            return WPResponse.Success();
        }

        [HttpDelete("DEBUG/resetDepartaments")]
        public async Task<IActionResult> ResetChatViewReports()
        {

            await _departamentRepository.DeleteAll();
            return WPResponse.Success();
        }
    }
}
