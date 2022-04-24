using WorkPortalAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkPortalAPI.Repositories
{
    public interface IMessageRepository
    {
        Task<IEnumerable<Message>> GetAll();
        Task<IEnumerable<Message>> GetAllFromChat(int chatId);
        Task<IEnumerable<Message>> GetFromChatSince(int chatId, string lastMessageUUID);
        Task<Message> Create(Message message);
        Task Update(Message message);
        Task Delete(string UUID);
    }
}
