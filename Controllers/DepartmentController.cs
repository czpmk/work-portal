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
    public class DepartmentController : ControllerBase
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IChatRepository _chatRepository;
        private readonly IDepartmentRepository _departamentRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IAuthRepository _authRepository;
        private readonly IUserRepository _userRepository;

        public DepartmentController(IUserRepository userRepository, IRoleRepository roleRepository, IChatRepository chatRepository, IAuthRepository authRepository, ICompanyRepository companyRepository, IDepartmentRepository departamentRepository)
        {
            this._roleRepository = roleRepository;
            this._chatRepository = chatRepository;
            this._authRepository = authRepository;
            this._companyRepository = companyRepository;
            this._departamentRepository = departamentRepository;
            this._userRepository = userRepository;
        }

        [HttpGet("getAll")]
        public async Task<IActionResult> GetAll(string token)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var invokingUser = await _authRepository.GetUserByToken(token);

            if (!invokingUser.IsAdmin)
                return WPResponse.AccessDenied("departaments");

            return WPResponse.Success(await _departamentRepository.Get());
        }

        [HttpGet]
        public async Task<IActionResult> Get(string token)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var invokingUser = await _authRepository.GetUserByToken(token);
            var invokingUserRole = await _authRepository.GetUserRoleByToken(token);

            if (invokingUserRole == null || invokingUserRole.DepartmentId == null)
                return WPResponse.ArgumentDoesNotExist("user not assigned to any departament");

            return WPResponse.Success(await _departamentRepository.Get(invokingUserRole.DepartmentId));
        }

        [HttpGet("{companyId}")]
        public async Task<IActionResult> Get(int companyId, string token)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var invokingUser = await _authRepository.GetUserByToken(token);
            var invokingUserRole = await _authRepository.GetUserRoleByToken(token);

            if (!invokingUser.IsAdmin && invokingUserRole.CompanyId != companyId)
                return WPResponse.AccessDenied("departaments");

            if (!(await _companyRepository.Exists(companyId)))
                return WPResponse.ArgumentDoesNotExist("company");
            else
                return WPResponse.Success(await _departamentRepository.GetByCompanyId(companyId));
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(Department departament, string token)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var user = await _authRepository.GetUserByToken(token);
            var role = await _authRepository.GetUserRoleByToken(token);
            // ADMIN OR COMPANY ADMINISTRATOR (OWNER) ONLY
            if (!user.IsAdmin && role.Type != RoleType.COMPANY_OWNER)
                return WPResponse.AccessDenied("Departament/Create");

            if (!user.IsAdmin && role.CompanyId != departament.CompanyId)
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
                DepartmentId = newDepartament.Id
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
        public async Task<IActionResult> SetHeadOfDepartament(int companyId, int departamentId, int userId, string token)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var user = await _authRepository.GetUserByToken(token);
            var role = await _authRepository.GetUserRoleByToken(token);
            // ADMIN OR COMPANY ADMINISTRATOR (OWNER) ONLY
            if (!user.IsAdmin && role.Type != RoleType.COMPANY_OWNER)
                return WPResponse.AccessDenied("Departament/Create");

            if (!user.IsAdmin && role.CompanyId != companyId)
                return WPResponse.AccessDenied("Departament/Create");

            if (!(await _companyRepository.Exists(companyId)))
                return WPResponse.ArgumentDoesNotExist("CompanyId");

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

            await _departamentRepository.GrantOwnership(newHead, RoleType.HEAD_OF_DEPARTMENT, departament.Id);
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
