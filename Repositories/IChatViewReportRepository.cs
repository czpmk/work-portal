using WorkPortalAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkPortalAPI.Repositories
{
    interface IChatViewReportRepository
    {
        Task<ChatViewReport> Get(int id);
        Task<ChatViewReport> Create(ChatViewReport chatViewReport);
        Task Update(ChatViewReport chatViewReport);
        Task Delete(int id);
        Task<Boolean> Exists(int id);
        Task<string> GetLastSeenMessageID(int chatId);
    }
}
