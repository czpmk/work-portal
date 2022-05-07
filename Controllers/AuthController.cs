using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkPortalAPI.Repositories;
using WorkPortalAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace WorkPortalAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;
        private readonly IUserRepository _userRepository;

        public AuthController(IAuthRepository authRepository, IUserRepository userRepository)
        {
            this._authRepository = authRepository;
            this._userRepository = userRepository;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(Credentials credentials)
        {
            //find user from credentials
            List<User> foundUsers = await _authRepository.GetUsersByEmail(credentials.Email);

            //if more than one user found
            if (foundUsers.Count > 1)
                return WPResponse.Custom(ReturnCode.INTERNAL_ERROR);

            //if user not found
            if (foundUsers.Count != 1)
                return WPResponse.Custom(ReturnCode.AUTHENTICATION_INVALID);

            User user = foundUsers.First();
            //if password is invalid return error
            if (Utils.GetSHA256HashOf(user.Salt + credentials.Password) != user.Password)
                return WPResponse.Custom(ReturnCode.AUTHENTICATION_INVALID);

            // all ok
            string token = await _authRepository.CreateSession(user.Id);
            return WPResponse.Success(token);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(User user)
        {
            var validLangs = new List<string>() { "polish", "english" };
            user.Salt = Utils.NewUUID();

            var passwordRaw = user.Password;
            user.Password = Utils.GetSHA256HashOf(user.Salt + passwordRaw);

            if (user.IsAdmin != true)
                user.IsAdmin = false;

            // validate
            if (!new EmailAddressAttribute().IsValid(user.Email))
                return WPResponse.ArgumentInvalid("email");

            if ((await _authRepository.GetUsersByEmail(user.Email)).Any())
                return WPResponse.ArgumentInvalid("email");

            if (!validLangs.Contains(user.Language))
                return WPResponse.ArgumentInvalid("language");

            await _authRepository.CreateUser(user);
            return await Login(new Credentials { Email = user.Email, Password = passwordRaw });
            // Shall it log in by default? If not use this line
            //return WPResponse.Success();
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout(String token)
        {
            //find user from credentials
            List<Session> foundSessions = await _authRepository.GetSessionsByToken(token);

            if (await _authRepository.SessionValid(token))
            {
                var sessions = await _authRepository.GetSessionsByToken(token);
                foreach (var s in sessions)
                    await _authRepository.TerminateSession(s);

                return WPResponse.Success();
            }
            else
            {
                return WPResponse.ArgumentInvalid("token");
            }
        }
    }
}
