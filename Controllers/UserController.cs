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
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthRepository _authRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IDepartmentRepository _departamentRepository;
        private readonly IChatViewReportRepository _chatViewReportRepository;
        private readonly IChatRepository _chatRepository;

        public UserController(IAuthRepository authRepository, IUserRepository userRepository, IRoleRepository roleRepository,
                                ICompanyRepository companyRepository, IDepartmentRepository departamentRepository,
                                IChatViewReportRepository chatViewReportRepository, IChatRepository chatRepository)
        {
            this._userRepository = userRepository;
            this._authRepository = authRepository;
            this._roleRepository = roleRepository;
            this._companyRepository = companyRepository;
            this._departamentRepository = departamentRepository;
            this._chatViewReportRepository = chatViewReportRepository;
            this._chatRepository = chatRepository;
        }

        [HttpGet("DEBUG")]
        public async Task<IActionResult> GetDebug()
        {
            return WPResponse.Success(await _userRepository.Get());
        }

        [HttpGet("DEBUG/{id}")]
        public async Task<IActionResult> GetDebug(int id)
        {
            var user = await _userRepository.Get(id);
            if (user == null)
                return WPResponse.ArgumentDoesNotExist("id");
            else
                return WPResponse.Success(user);
        }

        [HttpGet("DEBUG/myUserInfo")]
        public async Task<IActionResult> GetMyUserInfo(string token)
        {
            var user = await _authRepository.GetUserByToken(token);
            return WPResponse.Success(user);
        }

        [HttpGet("DEBUG/myRoleInfo")]
        public async Task<IActionResult> GetMyRoleInfo(string token)
        {
            var role = await _authRepository.GetUserRoleByToken(token);
            return WPResponse.Success(role);
        }

        [HttpGet("info")]
        public async Task<IActionResult> Get(string token)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var requestingUser = await _authRepository.GetUserByToken(token);
            var requestingUsersRole = await _roleRepository.GetByUserId(requestingUser.Id);

            if (requestingUser.IsAdmin)
            {
                return WPResponse.Success(await _userRepository.GetInfoForUsers(null, null, null));
            }
            else
            {
                switch (requestingUsersRole.Type)
                {
                    case RoleType.COMPANY_OWNER:
                        return WPResponse.Success(await _userRepository.GetInfoForUsers(null, requestingUsersRole.CompanyId, null));
                    case RoleType.HEAD_OF_DEPARTMENT:
                        return WPResponse.Success(await _userRepository.GetInfoForUsers(null, requestingUsersRole.CompanyId, requestingUsersRole.DepartmentId));
                    case RoleType.USER:
                    default:
                        return WPResponse.Success(await _userRepository.GetInfoForUsers(requestingUser.Id, null, null));
                }
            }
        }

        [HttpGet("info/{userId}")]
        public async Task<IActionResult> GetForUser(string token, int userId)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            return WPResponse.Success((await _userRepository.GetInfoForUsers(userId, null, null)).FirstOrDefault());
        }

        [HttpGet("myInfo")]
        public async Task<IActionResult> GetMyInfo(string token)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var requestingUser = await _authRepository.GetUserByToken(token);

            return WPResponse.Success((await _userRepository.GetInfoForUsers(requestingUser.Id, null, null)).FirstOrDefault());
        }

        [HttpPut("create")]
        public async Task<IActionResult> CreateUser(User user, string token, int companyId, int departamentId, int roleType)
        {

            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var requestingUser = await _authRepository.GetUserByToken(token);
            var requestingUsersRole = await _authRepository.GetUserRoleByToken(token);

            //Privilege check
            var validChecks = 0;

            if (requestingUsersRole.Type == RoleType.COMPANY_OWNER &&
                requestingUsersRole.CompanyId == companyId)
                validChecks++;

            else if (requestingUser.IsAdmin)
                validChecks++;

            if (validChecks == 0)
                return WPResponse.AccessDenied("access level");
            // ***

            user.Salt = Guid.NewGuid().ToString().Replace("-", "");
            user.Password = Utils.GetSHA256HashOf(user.Salt + user.Password);

            //check if invoking user has privilege to create administrator account
            if ((!requestingUser.IsAdmin) && user.IsAdmin)
            {
                return WPResponse.AccessDenied("IsAdmin");
            }

            //check if user already exists
            if (await _userRepository.Exists(user.Email))
            {
                return WPResponse.ArgumentAlreadyExists("Email");
            }

            //check if provided company exists
            if (!(await _companyRepository.Exists(companyId)))
            {
                return WPResponse.ArgumentDoesNotExist("companyId");
            }

            //check if provided departament exists
            if (!(await _departamentRepository.Exists(departamentId)))
            {
                return WPResponse.ArgumentDoesNotExist("departamentId");
            }

            //check if provided role exists
            if (!Enum.IsDefined(typeof(RoleType), roleType))
            {
                return WPResponse.ArgumentDoesNotExist("roleTypeId");
            }

            var createdUser = await _userRepository.Create(user);

            var createdUserRole = new Role { 
                CompanyId = companyId,
                DepartmentId = departamentId,
                UserId = createdUser.Id,
                Type = (RoleType) roleType
            };

            await _roleRepository.Create(createdUserRole);

            createdUser.Password = null;
            createdUser.Salt = null;

            if (await _companyRepository.Exists(createdUserRole.CompanyId) && await _chatRepository.GetCompanyChat(createdUserRole.CompanyId) != null)
            {
                var companyChat = await _chatRepository.GetCompanyChat(createdUserRole.CompanyId);
                await _chatViewReportRepository.Create(createdUser.Id, companyChat.Id);
            }

            if (await _departamentRepository.Exists(createdUserRole.DepartmentId) &&
                await _chatRepository.GetDepartamentChat(createdUserRole.CompanyId, createdUserRole.DepartmentId) != null)
            {
                var departamentChat = await _chatRepository.GetDepartamentChat(createdUserRole.CompanyId, createdUserRole.DepartmentId);
                await _chatViewReportRepository.Create(createdUser.Id, departamentChat.Id);
            }

            return WPResponse.Success(createdUser);
        }

        [HttpPatch("edit")]
        public async Task<IActionResult> EditUserSelf(UserInfo newUserInfo, string token)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var requestingUser = await _authRepository.GetUserByToken(token);

            //set new values
            if (newUserInfo.Email != null && !requestingUser.Email.Equals(newUserInfo.Email))
            {
                requestingUser.Email = newUserInfo.Email;
            }
            if (newUserInfo.FirstName != null && !requestingUser.FirstName.Equals(newUserInfo.FirstName))
            {
                requestingUser.FirstName = newUserInfo.FirstName;
            }
            if (newUserInfo.Surname != null && !requestingUser.Surname.Equals(newUserInfo.Surname))
            {
                requestingUser.Surname = newUserInfo.Surname;
            }
            if (newUserInfo.IsAdmin != null && !requestingUser.IsAdmin.Equals(newUserInfo.IsAdmin))
            {
                if (!requestingUser.IsAdmin) return WPResponse.AccessDenied("IsAdmin");
                requestingUser.IsAdmin = newUserInfo.IsAdmin;
            }
            if (newUserInfo.Language != null && !requestingUser.Language.Equals(newUserInfo.Language))
            {
                requestingUser.Language = newUserInfo.Language;
            }

            await _userRepository.Update(requestingUser);

            return WPResponse.Success();
        }

        [HttpPatch("edit/{userId}")]
        public async Task<IActionResult> EditUser(UserInfo newUserInfo, string token, int userId)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var requestingUser = await _authRepository.GetUserByToken(token);
            var requestingUsersRole = await _authRepository.GetUserRoleByToken(token);

            var targetUser = await _userRepository.Get(userId);
            var targetUsersRole = await _roleRepository.GetByUserId(userId);

            //Privilege check
            var validChecks = 0;

            if (requestingUsersRole.Type == RoleType.COMPANY_OWNER &&
                requestingUsersRole.CompanyId == targetUsersRole.CompanyId)
                validChecks++;

            else if (requestingUser.IsAdmin)
                validChecks++;

            else if (requestingUser.Id == targetUser.Id)
                validChecks++;

            if (validChecks == 0)
                return WPResponse.AccessDenied("access level");
            // ***

            //check if user exists
            if (!(await _userRepository.Exists(userId)))
            {
                return WPResponse.ArgumentDoesNotExist("userId");
            }

            //set new values
            if (newUserInfo.Email != null && !targetUser.Email.Equals(newUserInfo.Email))
            {
                targetUser.Email = newUserInfo.Email;
            }
            if (newUserInfo.FirstName != null && !targetUser.FirstName.Equals(newUserInfo.FirstName))
            {
                targetUser.FirstName = newUserInfo.FirstName;
            }
            if (newUserInfo.Surname != null && !targetUser.Surname.Equals(newUserInfo.Surname))
            {
                targetUser.Surname = newUserInfo.Surname;
            }
            if (newUserInfo.IsAdmin != null && !targetUser.IsAdmin.Equals(newUserInfo.IsAdmin))
            {
                if (!requestingUser.IsAdmin) return WPResponse.AccessDenied("IsAdmin");
                targetUser.IsAdmin = newUserInfo.IsAdmin;
            }
            if (newUserInfo.Language != null && !targetUser.Language.Equals(newUserInfo.Language))
            {
                targetUser.Language = newUserInfo.Language;
            }

            await _userRepository.Update(targetUser);

            return WPResponse.Success();
        }

        [HttpPatch("changeRole/{userId}")]
        public async Task<IActionResult> ChangeRole(string token, int companyId, int departamentId, int roleType, int userId)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var requestingUser = await _authRepository.GetUserByToken(token);
            var requestingUsersRole = await _authRepository.GetUserRoleByToken(token);

            var targetUser = await _userRepository.Get(userId);
            var targetUsersRole = await _roleRepository.GetByUserId(userId);

            //Privilege check
            var validChecks = 0;

            if (requestingUsersRole.Type == RoleType.COMPANY_OWNER &&
                companyId == null &&                        // company owner cannot change the user's assigned company
                requestingUsersRole.CompanyId == targetUsersRole.CompanyId && // can only change role of the users in his company
                roleType != (int)RoleType.COMPANY_OWNER)    // company owner cannot grant company owner access level
                validChecks++;

            else if (requestingUser.IsAdmin)
                validChecks++;

            if (validChecks == 0)
                return WPResponse.AccessDenied("access level");
            // ***

            //check if user exists
            if (!(await _userRepository.Exists(userId)))
            {
                return WPResponse.ArgumentDoesNotExist("userId");
            }

            //check if user's role exists
            if (targetUsersRole == null)
            {
                return WPResponse.ArgumentDoesNotExist("userId");
            }

            //check if provided company exists
            if (!(await _companyRepository.Exists(companyId)))
            {
                return WPResponse.ArgumentDoesNotExist("newCompanyId");
            }

            //check if provided departament exists
            if (!(await _departamentRepository.Exists(departamentId)))
            {
                return WPResponse.ArgumentDoesNotExist("newDepartamentId");
            }

            //check if provided role exists
            if (!Enum.IsDefined(typeof(RoleType), roleType))
            {
                return WPResponse.ArgumentDoesNotExist("newRoleTypeId");
            }

            var role = await _roleRepository.GetByUserId(userId);
            // move user to a proper chat
            if (role.CompanyId != companyId)
            {
                if (role.CompanyId != null)
                {
                    var oldCompanyChat = await _chatRepository.GetCompanyChat(role.CompanyId);
                    if (oldCompanyChat != null && (await _chatViewReportRepository.Exists(userId, oldCompanyChat.Id)))
                    {
                        var oldCvr = await _chatViewReportRepository.Get(userId, oldCompanyChat.Id);
                        if (oldCvr != null)
                            await _chatViewReportRepository.Delete(oldCvr.Id);
                    }
                }
                var companyChat = await _chatRepository.GetCompanyChat(companyId);
                await _chatViewReportRepository.Create(userId, companyChat.Id);

                // move to a proper departament when changing companies
                if (role.DepartmentId != null)
                {
                    var oldDepartamentChat = await _chatRepository.GetDepartamentChat(role.CompanyId, role.DepartmentId);
                    if (oldDepartamentChat != null && (await _chatViewReportRepository.Exists(userId, oldDepartamentChat.Id)))
                    {
                        var oldCvr = await _chatViewReportRepository.Get(userId, oldDepartamentChat.Id);
                        if (oldCvr != null)
                            await _chatViewReportRepository.Delete(oldCvr.Id);
                    }
                }
                var departamentChat = await _chatRepository.GetDepartamentChat(companyId, departamentId);
                await _chatViewReportRepository.Create(userId, departamentChat.Id);
            }
            else if (role.DepartmentId != departamentId)
            {
                if (role.DepartmentId != null)
                {
                    var oldDepartamentChat = await _chatRepository.GetDepartamentChat(role.CompanyId, role.DepartmentId);
                    if (oldDepartamentChat != null && (await _chatViewReportRepository.Exists(userId, oldDepartamentChat.Id)))
                    {
                        var oldCvr = await _chatViewReportRepository.Get(userId, oldDepartamentChat.Id);
                        if (oldCvr != null)
                            await _chatViewReportRepository.Delete(oldCvr.Id);
                    }
                }
                var departamentChat = await _chatRepository.GetDepartamentChat(companyId, departamentId);
                await _chatViewReportRepository.Create(userId, departamentChat.Id);
            }

            role.CompanyId = companyId;
            role.DepartmentId = departamentId;
            role.Type = (RoleType)roleType;

            await _roleRepository.Update(role);

            return WPResponse.Success();
        }


        [HttpDelete("delete/{userId}")]
        public async Task<IActionResult> DeleteUser(string token, int userId)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var requestingUser = await _authRepository.GetUserByToken(token);
            var requestingUsersRole = await _authRepository.GetUserRoleByToken(token);

            var targetUser = await _userRepository.Get(userId);
            var targetUsersRole = await _roleRepository.GetByUserId(userId);

            //Privilege check
            var validChecks = 0;

            if (requestingUsersRole.Type == RoleType.COMPANY_OWNER &&
                requestingUsersRole.CompanyId == targetUsersRole.CompanyId)
                validChecks++;

            else if (requestingUser.IsAdmin)
                validChecks++;

            if (validChecks == 0)
                return WPResponse.AccessDenied("access level");
            // ***

            if (!(await _userRepository.Exists(userId)))
            {
                return WPResponse.ArgumentDoesNotExist("userId");
            }

            await _userRepository.Delete(userId);

            return WPResponse.Success();
        }

        [HttpGet("find")]
        public async Task<IActionResult> FindUsers(string token, string? userName, int? companyId, int? departamentId)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            IEnumerable<dynamic> result = await _userRepository.FindUsers(userName, companyId, departamentId);

            return WPResponse.Success(result);
        }

        [HttpDelete("DEBUG/resetUsers")]
        public async Task<IActionResult> ResetChatViewReports()
        {

            await _userRepository.DeleteAll();
            return WPResponse.Success();
        }
    }
}