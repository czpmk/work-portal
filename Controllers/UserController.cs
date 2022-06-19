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
        private readonly IStatusRepository _statusRepository;
        private readonly IVacationRepository _vacationRepository;

        public UserController(IAuthRepository authRepository, IUserRepository userRepository, IRoleRepository roleRepository,
                                ICompanyRepository companyRepository, IDepartmentRepository departamentRepository,
                                IChatViewReportRepository chatViewReportRepository, IChatRepository chatRepository,
                                IStatusRepository statusRepository, IVacationRepository vacationRepository)
        {
            this._userRepository = userRepository;
            this._authRepository = authRepository;
            this._roleRepository = roleRepository;
            this._companyRepository = companyRepository;
            this._departamentRepository = departamentRepository;
            this._chatViewReportRepository = chatViewReportRepository;
            this._chatRepository = chatRepository;
            this._statusRepository = statusRepository;
            this._vacationRepository = vacationRepository;
        }

        [HttpGet("DEBUG")]
        public async Task<IActionResult> Get()
        {
            return WPResponse.Success(await _userRepository.Get());
        }

        [HttpGet("DEBUG/{id}")]
        public async Task<IActionResult> Get(int id)
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

        [HttpPut("create")]
        public async Task<IActionResult> CreateUser(User user, string token, int companyId, int departamentId, int roleTypeId)
        {

            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var invokingUser = await _authRepository.GetUserByToken(token);
            var invokingUserRole = await _authRepository.GetUserRoleByToken(token);

            user.Salt = Guid.NewGuid().ToString().Replace("-", "");
            user.Password = Utils.GetSHA256HashOf(user.Salt + user.Password);

            //check if invoking user has privilege to create administrator account
            if ((!invokingUser.IsAdmin) && user.IsAdmin)
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
            if (!Enum.IsDefined(typeof(RoleType), roleTypeId))
            {
                return WPResponse.ArgumentDoesNotExist("roleTypeId");
            }

            //TODO: Privilege Check

            var createdUser = await _userRepository.Create(user);

            Role createdUserRole = new();
            createdUserRole.CompanyId = companyId;
            createdUserRole.DepartmentId = departamentId;
            createdUserRole.UserId = createdUser.Id;
            createdUserRole.Type = (RoleType)roleTypeId;

            await _roleRepository.Create(createdUserRole);

            createdUser.Password = null;
            createdUser.Salt = null;

            return WPResponse.Success(createdUser);
        }

        [HttpPatch("edit")]
        public async Task<IActionResult> EditUserSelf(UserInfo userEdition, string token)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var invokingUser = await _authRepository.GetUserByToken(token);
            var userToEdit = await _authRepository.GetUserByToken(token);

            //set new values
            if (userEdition.Email != null && !userToEdit.Email.Equals(userEdition.Email))
            {
                userToEdit.Email = userEdition.Email;
            }
            if (userEdition.FirstName != null && !userToEdit.FirstName.Equals(userEdition.FirstName))
            {
                userToEdit.FirstName = userEdition.FirstName;
            }
            if (userEdition.Surname != null && !userToEdit.Surname.Equals(userEdition.Surname))
            {
                userToEdit.Surname = userEdition.Surname;
            }
            if (userEdition.IsAdmin != null && !userToEdit.IsAdmin.Equals(userEdition.IsAdmin))
            {
                if (!invokingUser.IsAdmin) return WPResponse.AccessDenied("IsAdmin");
                userToEdit.IsAdmin = userEdition.IsAdmin;
            }
            if (userEdition.Language != null && !userToEdit.Language.Equals(userEdition.Language))
            {
                userToEdit.Language = userEdition.Language;
            }

            await _userRepository.Update(userToEdit);

            return WPResponse.Success();
        }

        [HttpPatch("edit/{userId}")]
        public async Task<IActionResult> EditUser(UserInfo newUserInfo, string token, int userId)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var invokingUser = await _authRepository.GetUserByToken(token);
            var invokingUserRole = await _authRepository.GetUserRoleByToken(token);

            //check if user exists
            if (!(await _userRepository.Exists(userId)))
            {
                return WPResponse.ArgumentDoesNotExist("userId");
            }

            var oldUserInfo = await _userRepository.Get(userId);

            //set new values
            if (newUserInfo.Email != null && !oldUserInfo.Email.Equals(newUserInfo.Email))
            {
                oldUserInfo.Email = newUserInfo.Email;
            }
            if (newUserInfo.FirstName != null && !oldUserInfo.FirstName.Equals(newUserInfo.FirstName))
            {
                oldUserInfo.FirstName = newUserInfo.FirstName;
            }
            if (newUserInfo.Surname != null && !oldUserInfo.Surname.Equals(newUserInfo.Surname))
            {
                oldUserInfo.Surname = newUserInfo.Surname;
            }
            if (newUserInfo.IsAdmin != null && !oldUserInfo.IsAdmin.Equals(newUserInfo.IsAdmin))
            {
                if (!invokingUser.IsAdmin) return WPResponse.AccessDenied("IsAdmin");
                oldUserInfo.IsAdmin = newUserInfo.IsAdmin;
            }
            if (newUserInfo.Language != null && !oldUserInfo.Language.Equals(newUserInfo.Language))
            {
                oldUserInfo.Language = newUserInfo.Language;
            }

            //TODO: Privilege check

            await _userRepository.Update(oldUserInfo);

            return WPResponse.Success();
        }

        [HttpPatch("changeRole/{userId}")]
        public async Task<IActionResult> ChangeRole(string token, int newCompanyId, int newDepartamentId, int newRoleTypeId, int userId)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var invokingUser = await _authRepository.GetUserByToken(token);
            var invokingUserRole = await _authRepository.GetUserRoleByToken(token);

            //check if user exists
            if (!(await _userRepository.Exists(userId)))
            {
                return WPResponse.ArgumentDoesNotExist("userId");
            }

            //check if provided company exists
            if (!(await _companyRepository.Exists(newCompanyId)))
            {
                return WPResponse.ArgumentDoesNotExist("newCompanyId");
            }

            //check if provided departament exists
            if (!(await _departamentRepository.Exists(newDepartamentId)))
            {
                return WPResponse.ArgumentDoesNotExist("newDepartamentId");
            }

            //check if provided role exists
            if (!Enum.IsDefined(typeof(RoleType), newRoleTypeId))
            {
                return WPResponse.ArgumentDoesNotExist("newRoleTypeId");
            }

            //TODO: Privilege check

            var role = await _roleRepository.GetByUserId(userId);
            // move user to a proper chat
            if (role.CompanyId != newCompanyId)
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
                var companyChat = await _chatRepository.GetCompanyChat(newCompanyId);
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
                var departamentChat = await _chatRepository.GetDepartamentChat(newCompanyId, newDepartamentId);
                await _chatViewReportRepository.Create(userId, departamentChat.Id);
            }
            else if (role.DepartmentId != newDepartamentId)
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
                var departamentChat = await _chatRepository.GetDepartamentChat(newCompanyId, newDepartamentId);
                await _chatViewReportRepository.Create(userId, departamentChat.Id);
            }

            role.CompanyId = newCompanyId;
            role.DepartmentId = newDepartamentId;
            role.Type = (RoleType)newRoleTypeId;

            await _roleRepository.Update(role);

            return WPResponse.Success();
        }


        [HttpDelete("delete/{userId}")]
        public async Task<IActionResult> DeleteUser(string token, int userId)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var invokingUser = await _authRepository.GetUserByToken(token);
            var invokingUserRole = await _authRepository.GetUserRoleByToken(token);

            if (!(await _userRepository.Exists(userId)))
            {
                return WPResponse.ArgumentDoesNotExist("userId");
            }

            //TODO: Privilege check

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

        private static double calculateWorkTime(List<Status> statuses)
        {
            double workTime = .0d;
            double breakTime = .0d;
            bool workStarted = false;
            DateTime workTimestamp = DateTime.MinValue;
            bool breakStarted = false;
            DateTime breakTimestamp = DateTime.MinValue;

            if (statuses.Count == 1) //there must be a minimum of 2 statuses to be valid
            {
                return -1;
            }

            statuses.Sort((a, b) => a.Timestamp.CompareTo(b.Timestamp));

            foreach (var status in statuses)
            {

                if (status.Type == StatusType.Work)
                {
                    if (!workStarted)
                    {
                        workStarted = true;
                        workTimestamp = status.Timestamp;
                    }
                    else if (breakStarted)
                    {
                        breakStarted = false;
                        breakTime += (double)(status.Timestamp.Ticks - breakTimestamp.Ticks) / TimeSpan.TicksPerHour;
                    }
                }
                else if (status.Type == StatusType.Break)
                {
                    if (!workStarted) return -1; //invalid status
                    if (!breakStarted)
                    {
                        breakStarted = true;
                        breakTimestamp = status.Timestamp;
                    }
                }
                else if (status.Type == StatusType.OutOfOffice)
                {
                    if (breakStarted)
                    {
                        breakStarted = false;
                        breakTime += (double)(status.Timestamp.Ticks - breakTimestamp.Ticks) / TimeSpan.TicksPerHour;
                    }
                    if (workStarted)
                    {
                        workStarted = false;
                        workTime += (double)(status.Timestamp.Ticks - workTimestamp.Ticks) / TimeSpan.TicksPerHour;
                    }
                }

            }


            return workTime - breakTime;
        }

        [HttpGet("getStatistics/{year}/{month}")]
        public async Task<IActionResult> getStatistics(string token, int year, int month)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var user = await _authRepository.GetUserByToken(token);
            var statuses = await _statusRepository.Get(user.Id, month, year);
            var vacations = await _vacationRepository.GetByUserId(user.Id);

            int vacationDays = 0;
            int workingDays = 0;
            double sumOfWorkTime = 0;

            for (var day = 1; day <= DateTime.DaysInMonth(year, month); day++)
            {
                //select one day of vacations
                DateTime tmpDate = new DateTime(year, month, day);
                var tmpVacations = vacations.Where(v => tmpDate >= v.StartDate
                                                    && tmpDate <= (new DateTime(v.EndDate.Ticks)).AddTicks(TimeSpan.TicksPerDay - 1)
                                                    && v.State == VacationRequestState.ACCEPTED).ToList();

                //if there is at least one vacation request - count it and continue
                if (tmpVacations.Count > 0)
                {
                    vacationDays += 1;
                    continue;
                }

                //select one day
                var tmpWork = statuses.Where(s => s.Timestamp.Day == day
                                            && s.Timestamp.Month == month
                                            && s.Timestamp.Year == year).ToList();

                //calculate worktime
                var workTime = calculateWorkTime(tmpWork);
                sumOfWorkTime += workTime;

                if (workTime > 0) //presence at work
                {
                    workingDays += 1;
                }
            }

            var result = new Dictionary<string, object>() { { "sumOfWorkTime", sumOfWorkTime }, { "workingDays", workingDays }, { "vacationDays", vacationDays } };

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