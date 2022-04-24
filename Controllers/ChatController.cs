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
    public class ChatController
    {
        private readonly IAuthRepository _authRepository;
        private readonly IChatRepository _chatRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IChatViewReportRepository _chatViewReportRepository;

        public ChatController(IAuthRepository authRepository, IChatRepository chatRepository, IMessageRepository messageRepository, IChatViewReportRepository chatViewReportRepository)
        {
            this._authRepository = authRepository;
            this._chatRepository = chatRepository;
            this._messageRepository = messageRepository;
            this._chatViewReportRepository = chatViewReportRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMessages(int chatId, string token)
        {
            // TODO: check if action legal (user can view the messages)

            if (!(await _chatRepository.Exists(chatId)))
                return WPResponse.CreateArgumentInvalidResponse("chat_id");

            var messages = await _chatRepository.GetMessages(chatId);
            return WPResponse.Create(messages);
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

            var requestingUser = await _authRepository.FindUserByToken(token);
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
