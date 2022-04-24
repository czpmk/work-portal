﻿using Microsoft.AspNetCore.Http;
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
            List<User> foundUsers = await _authRepository.FindUsersByEmail(credentials.Email);

            //if more than one user found
            if (foundUsers.Count > 1)
                return WPResponse.Create(ReturnCode.INTERNAL_ERROR);

            //if user not found
            if (foundUsers.Count != 1)
                return WPResponse.Create(ReturnCode.AUTHENTICATION_INVALID);

            User user = foundUsers.First();
            //if password is invalid return error
            if (Utils.GetSHA256HashOf(user.Salt + credentials.PasswordHash) != user.Password)
                return WPResponse.Create(ReturnCode.AUTHENTICATION_INVALID);

            // all ok
            string token = await _authRepository.CreateSession(user.Id);
            return WPResponse.Create(token);
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
                return WPResponse.CreateArgumentInvalidResponse("email");

            if ((await _authRepository.FindUsersByEmail(user.Email)).Any())
                return WPResponse.CreateArgumentInvalidResponse("email");

            if (!validLangs.Contains(user.Language))
                return WPResponse.CreateArgumentInvalidResponse("language");

            await _authRepository.CreateUser(user);
            return await Login(new Credentials { Email = user.Email, PasswordHash = passwordRaw });
            // Shall it log in by default? If not use this line
            //return WPResponse.Create(ReturnCode.SUCCESS);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout(String token)
        {
            //find user from credentials
            List<Session> foundSessions = await _authRepository.FindSessionsByToken(token);

            if (await _authRepository.SessionValid(token))
            {
                var sessions = await _authRepository.FindSessionsByToken(token);
                foreach (var s in sessions)
                    await _authRepository.TerminateSession(s);

                return WPResponse.Create(ReturnCode.SUCCESS);
            }
            else
            {
                return WPResponse.CreateArgumentInvalidResponse("token");
            }
        }
    }
}
