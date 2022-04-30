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

            return WPResponse.Custom(newChat);
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

            return WPResponse.Custom(status);
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

            return WPResponse.Custom(await _chatRepository.HasMessageOlderThan(chatId, messageUUID));
        }

        [HttpPost("addMessage")]
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
            return WPResponse.Custom(newMessage);
        }

        [HttpPost("getMessages")]
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

            return WPResponse.Custom(messages);
        }

        [HttpPost("setStatus")]
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

            return WPResponse.Custom();
        }

        // DEBUG METHODS .........................................................

        //[HttpGet]
        //public async Task<IActionResult> GetAllMessages(int chatId, string token)
        //{
        //    // TODO: check if action legal (user can view the messages)

        //    if (!(await _chatRepository.Exists(chatId)))
        //        return WPResponse.ArgumentInvalid("chat_id");

        //    var messages = await _chatRepository.GetMessages(chatId);
        //    return WPResponse.Custom(messages);
        //}

        //[HttpPost("addUserToChat")]
        //public async Task<IActionResult> AddUserToChat(int chatId, int userId, string token)
        //{
        //    // TODO: check if action legal (user can view the messages)

        //    if (!(await _chatRepository.Exists(chatId)))
        //        return WPResponse.ArgumentDoesNotExist("Chat");

        //    if (await _chatRepository.IsPrivateChat(chatId))
        //        return WPResponse.OperationNotAllowed("Adding user manually to the private chat not allowed.");

        //    var user = await _authRepository.GetUserByToken(token);

        //    var chatViewReport = await _chatViewReportRepository.Create(userId, chatId);
        //    return WPResponse.Custom(chatViewReport);
        //}

        //[HttpGet("{nMessages}")]
        //public async Task<IActionResult> GetLastNMessages(int chatId, string token, int nMessages)
        //{
        //    // TODO: check if action legal (user can view the messages)

        //    if (!(await _chatRepository.Exists(chatId)))
        //        return WPResponse.ArgumentInvalid("chat_id");

        //    if (nMessages < 0)
        //        return WPResponse.ArgumentInvalid("n_messages");

        //    var messages = await _chatRepository.GetMessages(chatId, nMessages);
        //    return WPResponse.Custom(messages);
        //}

        //[HttpGet("sinceTimestamp/{timestamp}")]
        //public async Task<IActionResult> GetMessagesSinceTimestamp(int chatId, string token, DateTime timestamp)
        //{
        //    // TODO: check if action legal (user can view the messages)

        //    if (!(await _chatRepository.Exists(chatId)))
        //        return WPResponse.ArgumentInvalid("chat_id");

        //    var messages = await _chatRepository.GetMessagesSince(chatId, timestamp);
        //    return WPResponse.Custom(messages);
        //}

        //[HttpGet("sinceLastSeen/{lastMessageId}")]
        //public async Task<IActionResult> GetMessagesSinceLastSeen(int chatId, string token, string lastMessageId)
        //{
        //    // TODO: check if action legal (user can view the messages)

        //    if (!(await _chatRepository.Exists(chatId)))
        //        return WPResponse.ArgumentInvalid("chat_id");

        //    if (!(await _messageRepository.Exists(lastMessageId)))
        //        return WPResponse.ArgumentInvalid("lastMessageId");

        //    var lastMessage = await _messageRepository.Get(lastMessageId);

        //    var messages = await _chatRepository.GetMessagesSince(chatId, lastMessage);

        //    return WPResponse.Custom(messages);
        //}

        //[HttpGet("getNewMessageReport")]
        //public async Task<IActionResult> GetNewMessageReport(string token)
        //{
        //    // TODO: check if action legal (user can view the messages)

        //    var requestingUser = await _authRepository.GetUserByToken(token);
        //    if (requestingUser == null)
        //        return WPResponse.ArgumentInvalid("token");

        //    var viewingReports = await _chatViewReportRepository.GetReportsForUser(requestingUser.Id);

        //    var newMessageReports = new Dictionary<int, ChatViewStatus>();

        //    foreach (var r in viewingReports)
        //    {
        //        var status = ChatViewStatus.NEW_MESSAGES_AWAITING;

        //        if (!(await _chatViewReportRepository.Exists(requestingUser.Id, r.ChatId)))
        //        {
        //            status = ChatViewStatus.NEW_MESSAGES_AWAITING;
        //        }
        //        else
        //        {
        //            var lastViewed = await _chatViewReportRepository.GetLastSeenMessage(r.ChatId, requestingUser.Id);
        //            var lastPosted = await _chatRepository.GetLastMessage(r.ChatId);

        //            status = lastViewed.UUID == lastPosted.UUID ? ChatViewStatus.UP_TO_DATE : ChatViewStatus.NEW_MESSAGES_AWAITING;
        //        }
        //        newMessageReports.Add(r.ChatId, status);
        //    }
        //    return WPResponse.Custom(newMessageReports);
        //}
    }
}
