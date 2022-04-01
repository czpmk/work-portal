using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkPortalAPI.Repositories;
using WorkPortalAPI.Models;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Security.Cryptography;

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
        public async Task<Response> Login(Credentials credentials)
        {
            //find user from credentials
            List<User> foundUsers = await _authRepository.FindUsersByEmail(credentials.Email);

            //if more than one user found
            if (foundUsers.Count > 1)
            {
                return new Response(ReturnCode.INTERNAL_ERROR, null);
            }
            //if user not found
            else if (foundUsers.Count != 1)
            {
                return new Response(ReturnCode.INVALID_LOGIN_OR_PASSWORD, null);
            }

            User user = foundUsers.First();

            //if password is invalid return error
            if (ComputeSHA256Hash(user.Salt + credentials.PasswordHash) != user.Password)
            {
                return new Response(ReturnCode.INVALID_LOGIN_OR_PASSWORD, null);
            }

            //create session
            string token = await _authRepository.CreateSession(user.Id);
            return new Response(0, token);
        }

        [HttpPost("register")]
        public async Task<Response> Register(User user)
        {
            var validLangs = new List<string>() { "polish", "english" };
            user.Salt = Guid.NewGuid().ToString().Replace("-", "");

            var passwordRaw = user.Password;
            user.Password = ComputeSHA256Hash(user.Salt + passwordRaw);

            if (user.IsAdmin != true)
                user.IsAdmin = false;

            // validate
            if (!new EmailAddressAttribute().IsValid(user.Email))
                return new Response(ReturnCode.INVALID_ARGUMENT,
                                    new Dictionary<string, string>() { { "ArgumentName", "email" } });

            else if ((await _authRepository.FindUsersByEmail(user.Email)).Any())
                return new Response(ReturnCode.ARGUMENT_ALREADY_EXISTS,
                                    new Dictionary<string, string>() { { "ArgumentName", "email" } });

            else if (!validLangs.Contains(user.Language))
                return new Response(ReturnCode.INVALID_ARGUMENT,
                                    new Dictionary<string, string>() { { "ArgumentName", "language" } });

            // success
            else
                await _authRepository.CreateUser(user);

            // log in
            return await Login(new Credentials { Email = user.Email, PasswordHash = passwordRaw });
        }

        [HttpPost("logout")]
        public async Task<Response> Logout(String token)
        {
            //find user from credentials
            List<Session> foundSessions = await _authRepository.FindSessionsByToken(token);

            //if session not found return error
            if (foundSessions.Count == 0)
            {
                return new Response(ReturnCode.INVALID_SESSION_TOKEN, null);
            }

            //delete all found sessions
            foreach (Session session in foundSessions)
            {
                await _authRepository.InvalidateSession(session);
            }

            var r = new Response();
            return r;
        }

        private static string ComputeSHA256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
