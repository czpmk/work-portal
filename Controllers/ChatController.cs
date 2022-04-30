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
                return WPResponse.CreateAuthenticationInvalid();

            var user = _authRepository.GetUserByToken(token);
            var role = _roleRepository.Get(user.Id);

            if (await _chatRepository.Exists(chat))
                return WPResponse.CreateArgumentAlreadyExists("Chat");

            // only Private Chat can be created via this method
            if (!_chatRepository.IsPrivateChat(chat))
                return WPResponse.CreateArgumentInvalidResponse("Chat");

            // Chat can be created only by a user taking part in it
            if (chat.FirstUserId != user.Id && chat.SecondUserId != user.Id)
                return WPResponse.CreateOperationNotAllowed("Creating chat by a user not taking part in it");

            // One of the user ids does not exist
            if (!(await _userRepository.Exists(chat.FirstUserId.GetValueOrDefault())) ||
                !(await _userRepository.Exists(chat.SecondUserId.GetValueOrDefault())))
                return WPResponse.CreateArgumentDoesNotExist("UserId");

            // Same user id
            if (chat.FirstUserId == chat.SecondUserId)
                return WPResponse.CreateOperationNotAllowed("Chat member Id's cannot be identical");

            var newChat = await _chatRepository.Create(chat);
            // CREATE CHAT VIEW REPORT - CRUCIAL!!!
            await _chatViewReportRepository.Create(chat.FirstUserId.GetValueOrDefault(), chat.Id);
            await _chatViewReportRepository.Create(chat.SecondUserId.GetValueOrDefault(), chat.Id);

            return WPResponse.Create(newChat);
        }


        // DEBUG METHODS .........................................................

        [HttpGet]
        public async Task<IActionResult> GetAllMessages(int chatId, string token)
        {
            // TODO: check if action legal (user can view the messages)

            if (!(await _chatRepository.Exists(chatId)))
                return WPResponse.CreateArgumentInvalidResponse("chat_id");

            var messages = await _chatRepository.GetMessages(chatId);
            return WPResponse.Create(messages);
        }

        [HttpPost("addUserToChat")]
        public async Task<IActionResult> AddUserToChat(int chatId, int userId, string token)
        {
            // TODO: check if action legal (user can view the messages)

            if (!(await _chatRepository.Exists(chatId)))
                return WPResponse.CreateArgumentDoesNotExist("Chat");

            if (await _chatRepository.IsPrivateChat(chatId))
                return WPResponse.CreateOperationNotAllowed("Adding user manually to the private chat not allowed.");

            var user = await _authRepository.GetUserByToken(token);

            var chatViewReport = await _chatViewReportRepository.Create(userId, chatId);
            return WPResponse.Create(chatViewReport);
        }

        [HttpGet("{nMessages}")]
        public async Task<IActionResult> GetLastNMessages(int chatId, string token, int nMessages)
        {
            // TODO: check if action legal (user can view the messages)

            if (!(await _chatRepository.Exists(chatId)))
                return WPResponse.CreateArgumentInvalidResponse("chat_id");

            if (nMessages < 0)
                return WPResponse.CreateArgumentInvalidResponse("n_messages");

            var messages = await _chatRepository.GetMessages(chatId, nMessages);
            return WPResponse.Create(messages);
        }

        [HttpGet("sinceTimestamp/{timestamp}")]
        public async Task<IActionResult> GetMessagesSinceTimestamp(int chatId, string token, DateTime timestamp)
        {
            // TODO: check if action legal (user can view the messages)

            if (!(await _chatRepository.Exists(chatId)))
                return WPResponse.CreateArgumentInvalidResponse("chat_id");

            var messages = await _chatRepository.GetMessagesSince(chatId, timestamp);
            return WPResponse.Create(messages);
        }

        [HttpGet("sinceLastSeen/{lastMessageId}")]
        public async Task<IActionResult> GetMessagesSinceLastSeen(int chatId, string token, string lastMessageId)
        {
            // TODO: check if action legal (user can view the messages)

            if (!(await _chatRepository.Exists(chatId)))
                return WPResponse.CreateArgumentInvalidResponse("chat_id");

            if (!(await _messageRepository.Exists(lastMessageId)))
                return WPResponse.CreateArgumentInvalidResponse("lastMessageId");

            var lastMessage = await _messageRepository.Get(lastMessageId);

            var messages = await _chatRepository.GetMessagesSince(chatId, lastMessage);

            return WPResponse.Create(messages);
        }

        [HttpGet("getNewMessageReport")]
        public async Task<IActionResult> GetNewMessageReport(string token)
        {
            // TODO: check if action legal (user can view the messages)

            var requestingUser = await _authRepository.GetUserByToken(token);
            if (requestingUser == null)
                return WPResponse.CreateArgumentInvalidResponse("token");

            var viewingReports = await _chatViewReportRepository.GetReportsForUser(requestingUser.Id);

            var newMessageReports = new Dictionary<int, ChatViewStatus>();

            foreach (var r in viewingReports)
            {
                var status = ChatViewStatus.NEW_MESSAGES_AWAITING;

                if (!(await _chatViewReportRepository.Exists(requestingUser.Id, r.ChatId)))
                {
                    status = ChatViewStatus.NEW_MESSAGES_AWAITING;
                }
                else
                {
                    var lastViewed = await _chatViewReportRepository.GetLastSeenMessage(r.ChatId, requestingUser.Id);
                    var lastPosted = await _chatRepository.GetLastMessage(r.ChatId);

                    status = lastViewed.UUID == lastPosted.UUID ? ChatViewStatus.UP_TO_DATE : ChatViewStatus.NEW_MESSAGES_AWAITING;
                }
                newMessageReports.Add(r.ChatId, status);
            }
            return WPResponse.Create(newMessageReports);
        }
    }
}
