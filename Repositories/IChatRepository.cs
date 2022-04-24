using WorkPortalAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkPortalAPI.Repositories
{
    public interface IChatRepository
    {
        Task<IEnumerable<Chat>> GetGroupChats();
        Task<IEnumerable<Chat>> GetGroupChats(int companyId);
        Task<IEnumerable<Chat>> GetGroupChats(int companyId, int departamentId);
        Task<IEnumerable<Chat>> GetPrivateChats();
        Task<IEnumerable<Chat>> GetPrivateChats(int userId);
        Task<IEnumerable<Chat>> GetPrivateChats(int firstUserId, int secondUserId);
        Task<Message> GetLastMessage(int chatId);
        Task<IEnumerable<Message>> GetMessages(int chatId, int n = 50);
        Task<IEnumerable<Message>> GetMessagesSince(int chatId, DateTime timestamp);
        Task<IEnumerable<Message>> GetMessagesSince(int chatId, Message lastMessage);
        Task<Chat> Create(Chat chat);
        Task Update(Chat chat);
        Task Delete(int id);
        Task<Boolean> Exists(int id);
    }
}
