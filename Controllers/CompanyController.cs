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
    public class CompanyController: ControllerBase
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IDepartamentRepository _departamentRepository;
        private readonly IAuthRepository _authRepository;
        private readonly IChatRepository _chatRepository;
        private readonly IChatViewReportRepository _chatViewReportRepository;


        public CompanyController(ICompanyRepository companyRepository, IDepartamentRepository departamentRepository, IAuthRepository authRepository, IChatRepository chatRepository, IChatViewReportRepository chatViewReportRepository)
        {
            this._companyRepository = companyRepository;
            this._departamentRepository = departamentRepository;
            this._authRepository = authRepository;
            this._chatRepository = chatRepository;
            this._chatViewReportRepository = chatViewReportRepository;
        }

        [HttpGet]
        public async Task<IEnumerable<Company>> Get()
        {
            return await _companyRepository.Get();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Company>> Get(int id)
        {
            return await _companyRepository.Get(id);
        }

        [HttpGet("TestMethodToDelete7")]
        public async Task<IEnumerable<Company>> TestMethodToDelete7()
        {
            return await _companyRepository.Get();
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

            var newCompany = _companyRepository.Create(company);

            // CREATE COMPANY CHAT
            var chat = new Chat() { CompanyId = newCompany.Id};
            var newChat = _chatRepository.Create(chat);

            return WPResponse.Custom(newCompany);
        }

        [HttpGet("delete")]
        public async Task<IActionResult> Delete(Company company, string token)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var user = await _authRepository.GetUserByToken(token);
            if (!user.IsAdmin)
                return WPResponse.AccessDenied("Company/Create");

            if (!(await _companyRepository.Exists(company.Id)))
                return WPResponse.ArgumentDoesNotExist("CompanyId");

            // REMOVE COMPANY CHAT
            var companyChat = await _chatRepository.GetCompanyChat(company.Id);
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

            return WPResponse.Custom();
        }
    }
}
