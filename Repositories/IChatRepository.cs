using WorkPortalAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkPortalAPI.Repositories
{
    public interface IChatRepository
    {
        Task<List<Chat>> Get();
        Task<List<Chat>> GetGroupChats();
        Task<Chat> GetCompanyChat(int companyId);
        Task<List<Chat>> GetDepartamentChats(int companyId);
        Task<Chat> GetDepartamentChat(int companyId, int departamentId);
        Task<List<Chat>> GetPrivateChats();
        Task<List<Chat>> GetPrivateChats(int userId);
        Task<Chat> GetPrivateChat(int firstUserId, int secondUserId);
        Task<Message> GetLastMessage(int chatId);
        Task<List<Message>> GetMessages(int chatId, int n = 50);
        Task<List<Message>> GetMessagesSince(int chatId, string UUID, int n = 50);
        Task<List<Message>> GetMessagesUntil(int chatId, string UUID, int n = 50);
        Task<List<Message>> GetMessagesInRange(int chatId, string startRangeUUID, string endRangeUUID);
        Task<Chat> Create(Chat chat);
        Task Update(Chat chat);
        Task Delete(int id);
        Task<Boolean> Exists(int id);
        Task<Boolean> Exists(Chat chat);
        Boolean IsPrivateChat(Chat chat);
        Task<Boolean> IsPrivateChat(int chatId);
        Boolean IsGroupChat(Chat chat);
        Task<Boolean> IsGroupChat(int chatId);
        Task<Boolean> MessageExistsInChat(int chatId, string messageUUID);
        Task<Boolean> HasMessageOlderThan(int chatId, string messageUUID);
    }
}
