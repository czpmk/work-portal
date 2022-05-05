﻿using Microsoft.AspNetCore.Http;
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
        private readonly IDepartamentRepository _departamentRepository;

        public UserController(IAuthRepository authRepository, IUserRepository userRepository, IRoleRepository roleRepository, ICompanyRepository companyRepository, IDepartamentRepository departamentRepository)
        {
            this._userRepository = userRepository;
            this._authRepository = authRepository;
            this._roleRepository = roleRepository;
            this._companyRepository = companyRepository;
            this._departamentRepository = departamentRepository;
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
            user.Password = Utils.GetSHA256HashOf(user.Password + user.Salt);

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
            createdUserRole.DepartamentId = departamentId;
            createdUserRole.UserId = createdUser.Id;
            createdUserRole.Type = (RoleType)roleTypeId;

            await _roleRepository.Create(createdUserRole);

            createdUser.Password = null;
            createdUser.Salt = null;

            return WPResponse.Success(createdUser);
        }

        [HttpPatch("edit")]
        public async Task<IActionResult> EditUserSelf(UserEdition userEdition, string token)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var invokingUser = await _authRepository.GetUserByToken(token);
            var userToEdit = await _authRepository.GetUserByToken(token);

            if (!userToEdit.Email.Equals(userEdition.Email))
            {
                userToEdit.Email = userEdition.Email;
            }
            if (!userToEdit.FirstName.Equals(userEdition.FirstName))
            {
                userToEdit.FirstName = userEdition.FirstName;
            }
            if (!userToEdit.Surname.Equals(userEdition.Surname))
            {
                userToEdit.Surname = userEdition.Surname;
            }
            if (!userToEdit.IsAdmin.Equals(userEdition.IsAdmin))
            {
                if (!invokingUser.IsAdmin) return WPResponse.AccessDenied("IsAdmin");
                userToEdit.IsAdmin = userEdition.IsAdmin;
            }
            if (!userToEdit.Language.Equals(userEdition.Language))
            {
                userToEdit.Language = userEdition.Language;
            }

            await _userRepository.Update(userToEdit);

            return WPResponse.Success();
        }

        [HttpPatch("edit/{userId}")]
        public async Task<IActionResult> EditUser(UserEdition userEdition, string token, int userId)
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

            var userToEdit = await _userRepository.Get(userId);

            //set new values
            if (userToEdit.Email != null && !userToEdit.Email.Equals(userEdition.Email))
            {
                userToEdit.Email = userEdition.Email;
            }
            if (userToEdit.FirstName != null && !userToEdit.FirstName.Equals(userEdition.FirstName))
            {
                userToEdit.FirstName = userEdition.FirstName;
            }
            if (userToEdit.Surname != null && !userToEdit.Surname.Equals(userEdition.Surname))
            {
                userToEdit.Surname = userEdition.Surname;
            }
            if (userToEdit.IsAdmin != null && !userToEdit.IsAdmin.Equals(userEdition.IsAdmin))
            {
                if (!invokingUser.IsAdmin) return WPResponse.AccessDenied("IsAdmin");
                userToEdit.IsAdmin = userEdition.IsAdmin;
            }
            if (userToEdit.Language != null && !userToEdit.Language.Equals(userEdition.Language))
            {
                userToEdit.Language = userEdition.Language;
            }

            //TODO: Privilege check

            await _userRepository.Update(userToEdit);

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
            role.CompanyId = newCompanyId;
            role.DepartamentId = newDepartamentId;
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
    }
}