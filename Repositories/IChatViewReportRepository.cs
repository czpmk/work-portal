using WorkPortalAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkPortalAPI.Repositories
{
    public interface IChatViewReportRepository
    {
        Task<List<ChatViewReport>> Get();
        Task<ChatViewReport> Get(int id);
        Task<ChatViewReport> Get(int userId, int chatId);
        Task<ChatViewReport> GetReportForUser(int chatId, int userId);
        Task<IEnumerable<ChatViewReport>> GetReportsForUser(int userId);
        Task<ChatViewReport> Create(ChatViewReport chatViewReport);
        Task<ChatViewReport> Create(int userId, int chatId);
        Task Update(ChatViewReport chatViewReport);
        Task Delete(int id);
        Task<Boolean> Exists(int id);
        Task<Boolean> Exists(int userId, int chatId);
        Task<Message> GetLastSeenMessage(int chatId, int userId);
        Task DeleteAll();
    }
}
