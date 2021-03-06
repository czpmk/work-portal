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
        private readonly IAuthRepository _authRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRepository _userRepository;

        public VacationController(IVacationRepository vacationRepository, IUserRepository userRepository, IAuthRepository authRepository, IRoleRepository roleRepository)
        {
            this._vacationRepository = vacationRepository;
            this._authRepository = authRepository;
            this._roleRepository = roleRepository;
            this._userRepository = userRepository;
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

        [HttpDelete("DEBUG/resetVacations/{id}")]
        public async Task<IActionResult> RemoveChatViewReport(int id)
        {
            if (!(await _vacationRepository.Exists(id)))
            {
                return WPResponse.ArgumentDoesNotExist("id");
            }

            else
            {
                await _vacationRepository.Delete(id);
                return WPResponse.Success();
            }
        }

        [HttpPut("createRequest")]
        public async Task<IActionResult> CreateVacationRequest(string token, Vacation vacation)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var user = await _authRepository.GetUserByToken(token);

            if (!Enum.IsDefined(typeof(VacationType), vacation.Type))
                return WPResponse.ArgumentDoesNotExist("type");

            if (vacation.StartDate.CompareTo(vacation.EndDate) > 0)
                return WPResponse.ArgumentInvalid("Start date can't be later than end date");

            vacation.ModificationTime = DateTime.Now;
            vacation.UserId = user.Id;
            vacation.State = VacationRequestState.PENDING;

            await _vacationRepository.Create(vacation);

            return WPResponse.Success();
        }

        [HttpPost("acceptRequest")]
        public async Task<IActionResult> AcceptVacationRequest(string token, int requestId)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var requestingUser = await _authRepository.GetUserByToken(token);
            var requestingUsersRole = await _roleRepository.GetByUserId(requestingUser.Id);

            var request = await _vacationRepository.Get(requestId);

            if (request == null)
                return WPResponse.ArgumentDoesNotExist("requestId");

            var targetUser = request.UserId;
            var targetUsersRole = await _roleRepository.GetByUserId(request.UserId);

            //Privilege check
            var validChecks = 0;

            if (requestingUsersRole.Type == RoleType.HEAD_OF_DEPARTMENT &&
                requestingUsersRole.CompanyId == targetUsersRole.CompanyId &&
                requestingUsersRole.DepartmentId == targetUsersRole.DepartmentId)
                validChecks++;

            else if (requestingUsersRole.Type == RoleType.COMPANY_OWNER &&
                requestingUsersRole.CompanyId == targetUsersRole.CompanyId)
                validChecks++;

            else if (requestingUser.IsAdmin)
                validChecks++;

            if (validChecks == 0)
                return WPResponse.AccessDenied("access level");
            // ***

            request.State = VacationRequestState.ACCEPTED;

            await _vacationRepository.Update(request);

            return WPResponse.Success();
        }

        [HttpPost("rejectRequest")]
        public async Task<IActionResult> RejectVacationRequest(string token, int requestId)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var requestingUser = await _authRepository.GetUserByToken(token);
            var requestingUsersRole = await _roleRepository.GetByUserId(requestingUser.Id);

            var request = await _vacationRepository.Get(requestId);

            if (request == null)
                return WPResponse.ArgumentDoesNotExist("requestId");

            var targetUser = request.UserId;
            var targetUsersRole = await _roleRepository.GetByUserId(request.UserId);

            //Privilege check
            var validChecks = 0;

            if (requestingUsersRole.Type == RoleType.HEAD_OF_DEPARTMENT &&
                requestingUsersRole.CompanyId == targetUsersRole.CompanyId &&
                requestingUsersRole.DepartmentId == targetUsersRole.DepartmentId)
                validChecks++;

            else if (requestingUsersRole.Type == RoleType.COMPANY_OWNER &&
                requestingUsersRole.CompanyId == targetUsersRole.CompanyId)
                validChecks++;

            else if (requestingUser.IsAdmin)
                validChecks++;

            if (validChecks == 0)
                return WPResponse.AccessDenied("access level");
            // ***

            request.State = VacationRequestState.REJECTED;

            await _vacationRepository.Update(request);

            return WPResponse.Success();
        }

        [HttpGet]
        public async Task<IActionResult> getVacationRequestSelf(string token)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var user = await _authRepository.GetUserByToken(token);
            var requests = await _vacationRepository.GetByUserId(user.Id);

            return WPResponse.Success(requests);
        }

        [HttpGet("getRequestsForApprover")]
        public async Task<IActionResult> getVacationRequests(string token)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var user = await _authRepository.GetUserByToken(token);
            var role = await _roleRepository.GetByUserId(user.Id);

            if (role.Type == RoleType.USER && !user.IsAdmin)
                return WPResponse.AccessDenied("Must be a company owner, head of department or admin.");

            if (user.IsAdmin == true)
                return WPResponse.Success(await _vacationRepository.Get());

            else if (role.Type == RoleType.COMPANY_OWNER)
                return WPResponse.Success(await _vacationRepository.GetByCompanyId(role.CompanyId));

            else if (role.Type == RoleType.HEAD_OF_DEPARTMENT)
                return WPResponse.Success(await _vacationRepository.GetByDepartmentId(role.CompanyId, role.DepartmentId));

            return WPResponse.InternalError();
        }

        [HttpGet("getRequestsForApproverWithUserInfo")]
        public async Task<IActionResult> getVacationRequestsWithUserInfo(string token)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var user = await _authRepository.GetUserByToken(token);
            var role = await _roleRepository.GetByUserId(user.Id);

            List<Vacation> requests = null;

            if (role.Type == RoleType.USER && !user.IsAdmin)
                return WPResponse.AccessDenied("Must be a company owner, head of department or admin.");

            if (user.IsAdmin == true)
                requests = await _vacationRepository.Get();

            else if (role.Type == RoleType.COMPANY_OWNER)
                requests = await _vacationRepository.GetByCompanyId(role.CompanyId);

            else if (role.Type == RoleType.HEAD_OF_DEPARTMENT)
                requests = await _vacationRepository.GetByDepartmentId(role.CompanyId, role.DepartmentId);

            var users = new List<User>();
            foreach (var r in requests)
            {
                if (users.Any(x => r.UserId == x.Id))
                    continue;

                if (await _userRepository.Exists(r.UserId))
                {
                    users.Add(await _userRepository.Get(r.UserId));
                }
            }

            var results = requests.Select(x => new
            {
                Id = x.Id,
                UserId = x.UserId,
                Type = x.Type,
                State = x.State,
                ModificationTime = x.ModificationTime,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                FirstName = users.Any(u => u.Id == x.UserId) ? users.Where(u => u.Id == x.UserId).FirstOrDefault().FirstName : null,
                LastName = users.Any(u => u.Id == x.UserId) ? users.Where(u => u.Id == x.UserId).FirstOrDefault().Surname : null,
            }
            );

            return WPResponse.Success(results);
        }
    }
}
