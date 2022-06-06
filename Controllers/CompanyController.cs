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
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IDepartmentRepository _departamentRepository;
        private readonly IAuthRepository _authRepository;
        private readonly IChatRepository _chatRepository;
        private readonly IChatViewReportRepository _chatViewReportRepository;
        private readonly IUserRepository _userRepository;


        public CompanyController(IUserRepository userRepository, ICompanyRepository companyRepository, IDepartmentRepository departamentRepository, IAuthRepository authRepository, IChatRepository chatRepository, IChatViewReportRepository chatViewReportRepository)
        {
            this._companyRepository = companyRepository;
            this._departamentRepository = departamentRepository;
            this._authRepository = authRepository;
            this._chatRepository = chatRepository;
            this._chatViewReportRepository = chatViewReportRepository;
            this._userRepository = userRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string token)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var invokingUser = await _authRepository.GetUserByToken(token);
            var invokingUserRole = await _authRepository.GetUserRoleByToken(token);

            if (invokingUserRole == null || invokingUserRole.CompanyId == null)
                return WPResponse.ArgumentDoesNotExist("user not assigned to any company");

            return WPResponse.Success(await _companyRepository.Get(invokingUserRole.CompanyId));
        }

        [HttpGet("getAll")]
        public async Task<IActionResult> GetAll(string token)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            return WPResponse.Success(await _departamentRepository.Get());
        }

        [HttpGet("DEBUG/{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var company = await _companyRepository.Get(id);
            if (company == null)
                return WPResponse.ArgumentDoesNotExist("id");
            else
                return WPResponse.Success(company);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(Company company, string token)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var user = await _authRepository.GetUserByToken(token);
            if (!user.IsAdmin)
                return WPResponse.AccessDenied("Company/Create");

            // check if company by the name exists
            if (await _companyRepository.Exists(company))
                return WPResponse.ArgumentAlreadyExists("Company");

            var newCompany = await _companyRepository.Create(company);

            // CREATE COMPANY CHAT
            var chat = new Chat() { CompanyId = newCompany.Id };
            var newChat = await _chatRepository.Create(chat);

            return WPResponse.Success(newCompany);
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> Delete(int companyId, string token)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var user = await _authRepository.GetUserByToken(token);
            if (!user.IsAdmin)
                return WPResponse.AccessDenied("Company/Create");

            if (!(await _companyRepository.Exists(companyId)))
                return WPResponse.ArgumentDoesNotExist("CompanyId");
            var company = await _companyRepository.Get(companyId);

            // REMOVE COMPANY CHAT
            var companyChat = await _chatRepository.GetCompanyChat(company.Id);
            if (companyChat != null)
                await _chatRepository.Delete(companyChat.Id);

            // REMOVE DEPARTAMENT CHATS
            var departamentChats = await _chatRepository.GetDepartamentChats(company.Id);
            foreach (var c in departamentChats)
                await _chatRepository.Delete(c.Id);

            // REMOVE DEPARTAMENTS
            var departaments = await _departamentRepository.GetByCompanyId(company.Id);
            foreach (var d in departaments)
                await _departamentRepository.Delete(d.Id);

            await _companyRepository.Delete(company.Id);

            return WPResponse.Success();
        }

        [HttpPut("SetCompanyOwner")]
        public async Task<IActionResult> SetCompanyOwner(int companyId, int userId, string token)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var user = await _authRepository.GetUserByToken(token);
            if (!user.IsAdmin)
                return WPResponse.AccessDenied("Company/Create");

            if (!(await _companyRepository.Exists(companyId)))
                return WPResponse.ArgumentDoesNotExist("company");
            var company = await _companyRepository.Get(companyId);

            if (!(await _userRepository.Exists(userId)))
                return WPResponse.ArgumentDoesNotExist("newOwnerId");
            var newOwner = await _userRepository.Get(userId);

            // get old owner
            var oldOwner = await _companyRepository.GetOwner(company);
            if (oldOwner != null)
            {
                await _companyRepository.RetractOwnership(oldOwner);
            }

            await _companyRepository.GrantOwnership(newOwner, RoleType.COMPANY_OWNER, company.Id);
            return WPResponse.Success();
        }

        [HttpDelete("DEBUG/resetCompany")]
        public async Task<IActionResult> ResetChatViewReports()
        {

            await _companyRepository.DeleteAll();
            return WPResponse.Success();
        }
    }
}
