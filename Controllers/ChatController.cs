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
    public class ChatController
    {
        private readonly IAuthRepository _authRepository;
        private readonly IUserRepository _userRepository;
        private readonly IChatRepository _chatRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IChatViewReportRepository _chatViewReportRepository;
        private readonly IRoleRepository _roleRepository;


        public ChatController(IAuthRepository authRepository, IUserRepository userRepository, IChatRepository chatRepository, IMessageRepository messageRepository, IChatViewReportRepository chatViewReportRepository, IRoleRepository roleRepository)
        {
            this._authRepository = authRepository;
            this._userRepository = userRepository;
            this._chatRepository = chatRepository;
            this._messageRepository = messageRepository;
            this._chatViewReportRepository = chatViewReportRepository;
            this._roleRepository = roleRepository;
        }

        [HttpPost("createPrivateChat")]
        public async Task<IActionResult> Create(Chat chat, string token)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var user = await _authRepository.GetUserByToken(token);
            var role = await _roleRepository.Get(user.Id);

            if (await _chatRepository.Exists(chat))
                return WPResponse.ArgumentAlreadyExists("Chat");

            // only Private Chat can be created via this method
            if (!_chatRepository.IsPrivateChat(chat))
                return WPResponse.ArgumentInvalid("Chat");

            // Chat can be created only by a user taking part in it
            if (chat.FirstUserId != user.Id && chat.SecondUserId != user.Id)
                return WPResponse.OperationNotAllowed("Creating chat by a user not taking part in it");

            // One of the user ids does not exist
            if (!(await _userRepository.Exists(chat.FirstUserId.GetValueOrDefault())) ||
                !(await _userRepository.Exists(chat.SecondUserId.GetValueOrDefault())))
                return WPResponse.ArgumentDoesNotExist("UserId");

            // Same user id
            if (chat.FirstUserId == chat.SecondUserId)
                return WPResponse.OperationNotAllowed("Chat member Id's cannot be identical");

            var newChat = await _chatRepository.Create(chat);
            // CREATE CHAT VIEW REPORT - CRUCIAL!!!
            await _chatViewReportRepository.Create(chat.FirstUserId.GetValueOrDefault(), chat.Id);
            await _chatViewReportRepository.Create(chat.SecondUserId.GetValueOrDefault(), chat.Id);

            return WPResponse.Success(newChat);
        }

        [HttpGet("getStatus")]
        public async Task<IActionResult> Refresh(string token)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var user = await _authRepository.GetUserByToken(token);

            var status = new Dictionary<int, bool>();

            var chatViewReports = await _chatViewReportRepository.GetReportsForUser(user.Id);
            foreach (var cvr in chatViewReports)
            {
                status.Add(cvr.ChatId, cvr.MessageUUID == (await _chatRepository.GetLastMessage(cvr.ChatId)).UUID);
            }

            return WPResponse.Success(status);
        }

        [HttpGet("olderMessageExists")]
        public async Task<IActionResult> OlderMessagesExist(int chatId, string messageUUID, string token)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var user = await _authRepository.GetUserByToken(token);

            // USER HAS ACCESS TO CHAT
            if (!(await _chatViewReportRepository.Exists(user.Id, chatId)))
                return WPResponse.AccessDenied("Chat");

            var report = await _chatViewReportRepository.GetReportForUser(chatId, user.Id);
            if (report == null)
                return WPResponse.InternalError();

            if (!(await _chatRepository.MessageExistsInChat(chatId, messageUUID)))
                return WPResponse.ArgumentDoesNotExist("messageUUID");

            return WPResponse.Success(await _chatRepository.HasMessageOlderThan(chatId, messageUUID));
        }

        [HttpPut("addMessage")]
        public async Task<IActionResult> AddMessage(Message message, string token)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var user = await _authRepository.GetUserByToken(token);

            if (!(await _chatViewReportRepository.Exists(user.Id, message.ChatId)))
                return WPResponse.AccessDenied("Chat");

            message.UUID = Utils.NewUUID();
            message.Timestamp = DateTime.Now;
            var newMessage = await _messageRepository.Create(message);
            return WPResponse.Success(newMessage);
        }

        [HttpGet("getMessages")]
        public async Task<IActionResult> GetMessages(int chatId, string token, string? startUUID, string? endUUID, int n = 20)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var user = await _authRepository.GetUserByToken(token);

            if (!(await _chatViewReportRepository.Exists(user.Id, chatId)))
                return WPResponse.AccessDenied("Chat");

            if (n < 1)
                return WPResponse.ArgumentInvalid("n");

            var messages = new List<Message>();

            // Get last n messages, regardles of users current situation
            if (startUUID == null && endUUID == null)
            {
                messages.AddRange(await _chatRepository.GetMessages(chatId, n));
            }
            // Get n messages since last seen one
            else if (startUUID != null && endUUID == null)
            {
                messages.AddRange(await _chatRepository.GetMessagesSince(chatId, startUUID, n));
            }
            // Get n messages before one
            else if (startUUID == null && endUUID != null)
            {
                messages.AddRange(await _chatRepository.GetMessagesUntil(chatId, endUUID, n));
            }
            // Get all messages in range
            else if (startUUID != null && endUUID != null)
            {
                messages.AddRange(await _chatRepository.GetMessagesInRange(chatId, startUUID, endUUID));
            }

            return WPResponse.Success(messages);
        }

        [HttpPut("setStatus")]
        public async Task<IActionResult> AddMessage(int chatId, string UUID, string token)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var user = await _authRepository.GetUserByToken(token);

            if (!(await _messageRepository.Exists(UUID)))
                return WPResponse.ArgumentDoesNotExist("messageUUID");

            var message = await _messageRepository.Get(UUID);

            if (!(await _chatViewReportRepository.Exists(user.Id, message.ChatId)))
                return WPResponse.AccessDenied("Chat");

            var cvr = new ChatViewReport()
            {
                ChatId = chatId,
                MessageUUID = UUID,
                UserId = user.Id
            };
            await _chatViewReportRepository.Create(cvr);

            return WPResponse.Success();
        }
    }
}
