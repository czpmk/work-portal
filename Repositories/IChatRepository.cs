using WorkPortalAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkPortalAPI.Repositories
{
    public interface IChatRepository
    {
        Task<List<Chat>> GetGroupChats();
        Task<List<Chat>> GetGroupChats(int companyId);
        Task<List<Chat>> GetGroupChats(int companyId, int departamentId);
        Task<List<Chat>> GetPrivateChats();
        Task<List<Chat>> GetPrivateChats(int userId);
        Task<List<Chat>> GetPrivateChats(int firstUserId, int secondUserId);
        Task<Message> GetLastMessage(int chatId);
        Task<List<Message>> GetMessages(int chatId, int n = 50);
        Task<List<Message>> GetMessagesSince(int chatId, DateTime timestamp);
        Task<List<Message>> GetMessagesSince(int chatId, Message lastMessage);
        Task<Chat> Create(Chat chat);
        Task Update(Chat chat);
        Task Delete(int id);
        Task<Boolean> Exists(int id);
        Task<Boolean> Exists(Chat chat);
        Boolean IsPrivateChat(Chat chat);
        Task<Boolean> IsPrivateChat(int chatId);
        Boolean IsGroupChat(Chat chat);
        Task<Boolean> IsGroupChat(int chatId);
    }
}
