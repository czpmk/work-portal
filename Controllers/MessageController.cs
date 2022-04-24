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
    public class MessageController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;
        private readonly IMessageRepository _messageRepository;

        public MessageController(IMessageRepository messageRepository, IAuthRepository authRepository)
        {
            this._authRepository = authRepository;
            this._messageRepository = messageRepository;
        }

        [HttpGet("all")]
        public async Task<IEnumerable<Message>> Get()
        {

            return await _messageRepository.GetAll();
        }

        [HttpGet("{chatId}")]
        public async Task<IEnumerable<Message>> Get(int chatId)
        {
            return await _messageRepository.GetAllFromChat(chatId);
        }

        [HttpGet("{chatId}/{messageUUID}")]
        public async Task<IEnumerable<Message>> Get(int chatId, string messageUUID)
        {
            return await _messageRepository.GetFromChatSince(chatId, messageUUID);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddMessage(Message message, String token)
        {
            var newUserId = Utils.NewUUID();

            if (!(await _authRepository.SessionValid(token)))
            {
                await _authRepository.TerminateSession(token);
                return WPResponse.Create(ReturnCode.AUTHENTICATION_INVALID);
            }

            _messageRepository.Create(message);

            return WPResponse.Create();
        }
    }
}
