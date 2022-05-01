using WorkPortalAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkPortalAPI.Repositories
{
    public class ChatViewReportRepository : IChatViewReportRepository
    {
        private readonly WPContext _context;
        public ChatViewReportRepository(WPContext context)
        {
            this._context = context;
        }
        public async Task<ChatViewReport> Create(ChatViewReport chatViewReport)
        {
            _context.ChatViewReports.Add(chatViewReport);
            await _context.SaveChangesAsync();
            return chatViewReport;
        }

        public async Task<ChatViewReport> Create(int userId, int chatId)
        {
            var chatViewReport = new ChatViewReport()
            {
                UserId = userId,
                ChatId = chatId,
                MessageUUID = null
            };

            return await Create(chatViewReport);
        }

        public async Task Delete(int id)
        {
            var chatViewReport = await _context.ChatViewReports.FindAsync(id);
            _context.ChatViewReports.Remove(chatViewReport);
            await _context.SaveChangesAsync();
        }

        public async Task<Boolean> Exists(int id)
        {
            return await _context.ChatViewReports.AnyAsync(c => c.Id == id);
        }

        public async Task<Boolean> Exists(int userId, int chatId)
        {
            return await _context.ChatViewReports.AnyAsync(c => c.UserId == userId && c.ChatId == chatId);
        }

        public async Task<List<ChatViewReport>> Get()
        {
            return await _context.ChatViewReports.ToListAsync();
        }

        public async Task<ChatViewReport> Get(int id)
        {
            return await _context.ChatViewReports.FindAsync(id);
        }

        public async Task<Message> GetLastSeenMessage(int chatId, int userId)
        {
            var report = await _context.ChatViewReports.Where(c => c.ChatId == chatId && c.UserId == userId).FirstOrDefaultAsync();
            if (report == null)
                return null;
            else
                return await _context.Messages.FindAsync(report.MessageUUID);
        }

        public async Task Update(ChatViewReport chatViewReport)
        {
            _context.Entry(chatViewReport).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<ChatViewReport> GetReportForUser(int chatId, int userId)
        {
            return await _context.ChatViewReports.Where(c => c.ChatId == chatId && c.UserId == userId).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<ChatViewReport>> GetReportsForUser(int userId)
        {
            return await _context.ChatViewReports.Where(c => c.UserId == userId).ToListAsync();
        }
    }
}
