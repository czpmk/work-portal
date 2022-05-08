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

        [HttpGet("DEBUG/chats")]
        public async Task<IActionResult> GetChats()
        {
            return WPResponse.Success(await _chatRepository.Get());
        }

        [HttpGet("DEBUG/messages")]
        public async Task<IActionResult> GetMessages()
        {
            return WPResponse.Success(await _messageRepository.Get());
        }

        [HttpGet("DEBUG/chatViewReports")]
        public async Task<IActionResult> GetChatViewReports()
        {
            return WPResponse.Success(await _chatViewReportRepository.Get());
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
            await _chatViewReportRepository.Create(newChat.FirstUserId.GetValueOrDefault(), newChat.Id);
            await _chatViewReportRepository.Create(newChat.SecondUserId.GetValueOrDefault(), newChat.Id);

            return WPResponse.Success(newChat);
        }

        [HttpGet("getStatus")]
        public async Task<IActionResult> RefreshAll(string token)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var user = await _authRepository.GetUserByToken(token);
            var status = new Dictionary<int, Object>();
            var chatViewReports = await _chatViewReportRepository.GetReportsForUser(user.Id);

            // DEBUG WARNING MESSAGES ahead
            var i = -10000000; // DEBUG - remove when chat creating and such is OK
            foreach (var cvr in chatViewReports)
            {
                if (cvr.ChatId == null) // DEBUG - remove when chat creating and such is OK
                {
                    status.Add(cvr.ChatId, String.Format("Invalid Chat View Repository entry {0}: ChatId = null", cvr.Id));
                    continue;
                }
                if (!(await _chatRepository.Exists(cvr.ChatId))) // DEBUG - The chat pointed to by Chat View Repository does not exist - remove when chat creating and such is OK
                {
                    status.Add(cvr.ChatId, String.Format("Invalid Chat View Repository entry {0}: The chat {1} does not exist", cvr.Id, cvr.ChatId));
                    continue;
                }

                var lastMessage = await _chatRepository.GetLastMessage(cvr.ChatId);
                if (lastMessage == null) // the chat is empty (no messages)
                {
                    if (cvr.MessageUUID == null)
                    {
                        status.Add(cvr.ChatId, new Dictionary<string, Object>() { { "upToDate", true }, { "lastMessageTimestamp", null } });
                        continue;
                    }
                    else //DEBUG - Another error in DB entries - no message found with the chatId selected, yet the chat view report points to some
                    {
                        cvr.MessageUUID = null;
                        await _chatViewReportRepository.Update(cvr);
                        status.Add(cvr.ChatId, String.Format("Invalid Chat View Repository entry {0}: " +
                            "The chat is empty, yet the CVR points to a not null message. Setting it to null", cvr.Id));
                        continue;
                    }
                }

                if (status.Keys.Contains(cvr.ChatId)) // DEBUG - ... and another...
                {
                    var mess = String.Format("WARNING: Duplicate ChatViewReport with chatId ({0}) found with message UUID {1}", cvr.ChatId, cvr.MessageUUID);
                    status.Add(i, mess);
                    i++;
                }
                else
                {
                    //status.Add(cvr.ChatId, cvr.MessageUUID == (await _chatRepository.GetLastMessage(cvr.ChatId)).UUID);
                    status.Add(cvr.ChatId, new Dictionary<string, Object>() { 
                        { "upToDate", cvr.MessageUUID == (await _chatRepository.GetLastMessage(cvr.ChatId)).UUID }, 
                        { "lastMessageTimestamp", lastMessage.Timestamp } 
                    });
                }
            }
            return WPResponse.Success(status);
        }

        [HttpGet("{chatId}/getStatus")]
        public async Task<IActionResult> RefreshSingleChat(string token, int chatId)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var user = await _authRepository.GetUserByToken(token);

            if (!(await _chatRepository.Exists(chatId))) // The chat does not exist
            {
                return WPResponse.ArgumentAlreadyExists("chatId");
            }

            var chatViewReports = await _chatViewReportRepository.GetReportsForUser(user.Id);
            var cvr = chatViewReports.Where(c => c.ChatId == chatId).FirstOrDefault();
            if (cvr == null)
            {
                return WPResponse.AccessDenied("chat");
            }

            var lastMessage = await _chatRepository.GetLastMessage(chatId);
            var status = new Dictionary<string, object>();
            if (lastMessage == null)
            {
                if (cvr.MessageUUID != null) // DEBUG - no messages found for chat, yet chat view report points to some - REMOVE
                {
                    cvr.MessageUUID = null;
                    await _chatViewReportRepository.Update(cvr);
                }
                status.Add("upToDate", true);
                status.Add("timestamp", null);
            }
            else
            {
                status.Add("upToDate", lastMessage.UUID == cvr.MessageUUID);
                status.Add("timestamp", lastMessage.Timestamp);
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
        public async Task<IActionResult> SetStatus(int chatId, string UUID, string token)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var user = await _authRepository.GetUserByToken(token);

            if (!(await _messageRepository.Exists(UUID)))
                return WPResponse.ArgumentDoesNotExist("messageUUID");

            var message = await _messageRepository.Get(UUID);

            if (!(await _chatViewReportRepository.Exists(user.Id, message.ChatId)))
                return WPResponse.AccessDenied("Chat");

            var cvr = await _chatViewReportRepository.GetReportForUser(chatId, user.Id);
            cvr.MessageUUID = UUID;
            await _chatViewReportRepository.Update(cvr);

            return WPResponse.Success();
        }

        [HttpGet("getChatsForUser")]
        public async Task<IActionResult> GetMyChats(string token)
        {
            if (!(await _authRepository.SessionValid(token)))
                return WPResponse.AuthenticationInvalid();

            var user = await _authRepository.GetUserByToken(token);

            var chatViewReports = await _chatViewReportRepository.GetReportsForUser(user.Id);
            var chats = new List<Dictionary<string, object>>();

            foreach (var cvr in chatViewReports)
            {
                chats.Add(
                    new Dictionary<string, object>() {
                        {"chat", await _chatRepository.Get(cvr.ChatId)},
                        {"description", await _chatRepository.GetChatDescriptionDictionary(cvr.ChatId)}
                    }
                    );
            }

            return WPResponse.Success(chats);
        }

        [HttpDelete("DEBUG/resetMessages")]
        public async Task<IActionResult> ResetMessages()
        {

            await _messageRepository.DeleteAll();
            return WPResponse.Success();
        }

        [HttpDelete("DEBUG/resetChats")]
        public async Task<IActionResult> ResetChats()
        {

            await _chatRepository.DeleteAll();
            return WPResponse.Success();
        }

        [HttpDelete("DEBUG/resetChatViewReports")]
        public async Task<IActionResult> ResetChatViewReports()
        {

            await _chatViewReportRepository.DeleteAll();
            return WPResponse.Success();
        }
    }
}
