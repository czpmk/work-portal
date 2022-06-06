using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkPortalAPI.Repositories;
using WorkPortalAPI.Models;
using Spire.Xls;
using System.IO;

namespace WorkPortalAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatusController
    {
        private readonly IStatusRepository _statusRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAuthRepository _authRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IVacationRepository _vacationRepository;

        public StatusController(IStatusRepository statusRepository, IAuthRepository authRepository, IRoleRepository roleRepository, IUserRepository userRepository, IVacationRepository vacationRepository)
        {
            this._authRepository = authRepository;
            this._statusRepository = statusRepository;
            this._roleRepository = roleRepository;
            this._userRepository = userRepository;
            this._vacationRepository = vacationRepository;

        }

        [HttpGet("")]
        public async Task<IActionResult> Get(string token)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var user = await _authRepository.GetUserByToken(token);
            var status = await _statusRepository.Get(user.Id);

            return WPResponse.Success(status);
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> Get(string token, int userId)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var requestingUser = await _authRepository.GetUserByToken(token);
            var requestingUsersRole = await _roleRepository.GetByUserId(requestingUser.Id);

            var targetUser = await _userRepository.Get(userId);
            var targetUsersRole = await _roleRepository.GetByUserId(targetUser.Id);

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

            if (!(await _userRepository.Exists(userId)))
                return WPResponse.ArgumentDoesNotExist("userId");

            var status = await _statusRepository.Get(userId);

            return WPResponse.Success(status);
        }

        [HttpGet("last")]
        public async Task<IActionResult> LastStatusOfUser(string token)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var user = await _authRepository.GetUserByToken(token);
            var status = await _statusRepository.Last(user.Id);

            return WPResponse.Success(status);
        }

        [HttpPut("setStatus")]
        public async Task<IActionResult> SetNewStatus(string token, int statusTypeId)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var user = await _authRepository.GetUserByToken(token);
            var lastStatus = await _statusRepository.Last(user.Id);

            var newStatus = new Status();

            newStatus.Timestamp = DateTime.Now;
            newStatus.Type = (StatusType)statusTypeId;
            newStatus.UserId = user.Id;

            if (lastStatus != null)
            {
                if (lastStatus.Type == newStatus.Type)
                {
                    return WPResponse.OperationNotAllowed("This status is already set! New status can't be same as last status of user.");
                }
                else if (lastStatus.Type == StatusType.OutOfOffice && newStatus.Type == StatusType.Break)
                {
                    return WPResponse.OperationNotAllowed("Can't set status from 'OutOfOffice' to 'Break'.");
                }
            }

            await _statusRepository.Create(newStatus);

            return WPResponse.Success();
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

        [HttpGet("export")]
        public async Task<IActionResult> ExportStatusesSelf(string token, int month, int year)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var user = await _authRepository.GetUserByToken(token);

            var statuses = await _statusRepository.Get(user.Id, month, year);

            //create worksheet and workbook, setup template
            Workbook workbook = new Workbook();
            workbook.Worksheets[0].Name = "Raport obecności";
            workbook.Worksheets[2].Remove();
            workbook.Worksheets[1].Remove();
            Worksheet sheet = workbook.Worksheets[0];
            sheet.Range["A1"].Text = "Dzień miesiąca";
            sheet.Range["B1"].Text = "Status obecności";
            sheet.Range["C1"].Text = "Czas pracy [h]";
            sheet.Range["E1"].Text = "Legenda:";
            sheet.Range["E2"].Text = "O - Obecny";
            sheet.Range["E3"].Text = "N - Nieobecny";
            sheet.Range["E4"].Text = "U - Urlop";
            sheet.Range["E5"].Text = "B - Błąd statusu";

            //TODO: Add vacations
            var vacations = await _vacationRepository.GetByUserId(user.Id);

            for (var day = 1; day <= DateTime.DaysInMonth(year, month); day++)
            {
                //select one day of vacations
                DateTime tmpDate = new DateTime(year, month, day);
                var tmpVacations = vacations.Where(v => tmpDate >= v.StartDate
                                                    && tmpDate <= (new DateTime(v.EndDate.Ticks)).AddTicks(TimeSpan.TicksPerDay - 1)
                                                    && v.State == VacationRequestState.ACCEPTED).ToList();

                //if there is at least one vacation request - write to worksheet and continue
                if (tmpVacations.Count > 0)
                {
                    sheet["A" + (day + 1).ToString()].Text = day.ToString();
                    sheet["B" + (day + 1).ToString()].Text = "U";
                    sheet["C" + (day + 1).ToString()].Text = "0";
                    continue;
                }

                //select one day
                var tmpWork = statuses.Where(s => s.Timestamp.Day == day
                                            && s.Timestamp.Month == month
                                            && s.Timestamp.Year == year).ToList();

                //calculate worktime
                var workTime = calculateWorkTime(tmpWork);

                //write to worksheet
                sheet["A" + (day + 1).ToString()].Text = day.ToString();

                if (workTime == -1) //status error
                {
                    sheet["B" + (day + 1).ToString()].Text = "B";
                    sheet["C" + (day + 1).ToString()].Text = "0";
                }
                else if (workTime == 0) //absence from work
                {
                    sheet["B" + (day + 1).ToString()].Text = "N";
                    sheet["C" + (day + 1).ToString()].Text = workTime.ToString();
                }
                else if (workTime > 0) //presence at work
                {
                    sheet["B" + (day + 1).ToString()].Text = "O";
                    sheet["C" + (day + 1).ToString()].Text = workTime.ToString();
                }
            }

            //convert workbook to byteArray
            byte[] byteArray;
            using (MemoryStream ms = new())
            {
                workbook.SaveToStream(ms);
                byteArray = ms.ToArray();
            }

            //convert byteArray to base64String
            var base64String = Convert.ToBase64String(byteArray, 0, byteArray.Length);

            return WPResponse.Success(base64String);
        }

        [HttpGet("export/{userId}")]
        public async Task<IActionResult> ExportStatuses(string token, int userId, int month, int year)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            if (!(await _userRepository.Exists(userId)))
                return WPResponse.ArgumentDoesNotExist("userId");

            var requestingUser = await _authRepository.GetUserByToken(token);
            var requestingUsersRole = await _roleRepository.GetByUserId(requestingUser.Id);

            var targetUser = await _userRepository.Get(userId);
            var targetUsersRole = await _roleRepository.GetByUserId(targetUser.Id);

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

            var user = await _userRepository.Get(userId);

            var statuses = await _statusRepository.Get(user.Id, month, year);

            //create worksheet and workbook, setup template
            Workbook workbook = new Workbook();
            workbook.Worksheets[0].Name = "Raport obecności";
            workbook.Worksheets[2].Remove();
            workbook.Worksheets[1].Remove();
            Worksheet sheet = workbook.Worksheets[0];
            sheet.Range["A1"].Text = "Dzień miesiąca";
            sheet.Range["B1"].Text = "Status obecności";
            sheet.Range["C1"].Text = "Czas pracy [h]";
            sheet.Range["E1"].Text = "Legenda:";
            sheet.Range["E2"].Text = "O - Obecny";
            sheet.Range["E3"].Text = "N - Nieobecny";
            sheet.Range["E4"].Text = "U - Urlop";
            sheet.Range["E5"].Text = "B - Błąd statusu";

            var vacations = await _vacationRepository.GetByUserId(user.Id);

            for (var day = 1; day <= DateTime.DaysInMonth(year, month); day++)
            {
                //select one day of vacations
                DateTime tmpDate = new DateTime(year, month, day);
                var tmpVacations = vacations.Where(v => tmpDate >= v.StartDate
                                                    && tmpDate <= (new DateTime(v.EndDate.Ticks)).AddTicks(TimeSpan.TicksPerDay - 1)
                                                    && v.State == VacationRequestState.ACCEPTED).ToList();

                //if there is at least one vacation request - write to worksheet and continue
                if (tmpVacations.Count > 0)
                {
                    sheet["A" + (day + 1).ToString()].Text = day.ToString();
                    sheet["B" + (day + 1).ToString()].Text = "U";
                    sheet["C" + (day + 1).ToString()].Text = "0";
                    continue;
                }

                //select one day of work
                var tmpWork = statuses.Where(s => s.Timestamp.Day == day
                                         && s.Timestamp.Month == month
                                         && s.Timestamp.Year == year).ToList();

                //calculate worktime
                var workTime = calculateWorkTime(tmpWork);

                //write to worksheet
                sheet["A" + (day + 1).ToString()].Text = day.ToString();

                if (workTime == -1) //status error
                {
                    sheet["B" + (day + 1).ToString()].Text = "B";
                    sheet["C" + (day + 1).ToString()].Text = "0";
                }
                else if (workTime == 0) //absence from work
                {
                    sheet["B" + (day + 1).ToString()].Text = "N";
                    sheet["C" + (day + 1).ToString()].Text = workTime.ToString();
                }
                else if (workTime > 0) //presence at work
                {
                    sheet["B" + (day + 1).ToString()].Text = "O";
                    sheet["C" + (day + 1).ToString()].Text = workTime.ToString();
                }
            }

            //convert workbook to byteArray
            byte[] byteArray;
            using (MemoryStream ms = new())
            {
                workbook.SaveToStream(ms);
                byteArray = ms.ToArray();
            }

            //convert byteArray to base64String
            var base64String = Convert.ToBase64String(byteArray, 0, byteArray.Length);

            return WPResponse.Success(base64String);
        }

        [HttpDelete("DEBUG/resetStatuses")]
        public async Task<IActionResult> ResetChatViewReports()
        {

            await _statusRepository.DeleteAll();
            return WPResponse.Success();
        }
    }
}
