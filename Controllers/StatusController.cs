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
    public class StatusController
    {
        private readonly IStatusRepository _statusRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAuthRepository _authRepository;
        private readonly IRoleRepository _roleRepository;

        public StatusController(IStatusRepository statusRepository, IAuthRepository authRepository, IRoleRepository roleRepository, IUserRepository userRepository)
        {
            this._authRepository = authRepository;
            this._statusRepository = statusRepository;
            this._roleRepository = roleRepository;
            this._userRepository = userRepository;
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
            
            var user = await _authRepository.GetUserByToken(token);
            var role = await _roleRepository.Get(user.Id);

            //TODO: Privilege check

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

            if (lastStatus != null) {
                if(lastStatus.Type == newStatus.Type)
                {
                    return WPResponse.OperationNotAllowed("This status is already set! New status can't be same as last status of user.");
                }
                else if(lastStatus.Type == StatusType.OutOfOffice && newStatus.Type == StatusType.Break)
                {
                    return WPResponse.OperationNotAllowed("Can't set status from 'OutOfOffice' to 'Break'.");
                }
            }

            await _statusRepository.Create(newStatus);

            return WPResponse.Success();
        }
    }
}
