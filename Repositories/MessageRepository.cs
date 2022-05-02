using WorkPortalAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace WorkPortalAPI.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly WPContext _context;
        public MessageRepository(WPContext context)
        {
            this._context = context;
        }

        public async Task<List<Message>> Get()
        {
            return await _context.Messages.ToListAsync();
        }

        public async Task<Message> Get(string UUID)
        {
            return await _context.Messages.FindAsync(UUID);
        }
        public async Task<Message> Create(Message message)
        {
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task Delete(string UUID)
        {
            var message = _context.Messages.FirstOrDefault(m => m.UUID == UUID);
            if (message != null)
            {
                _context.Messages.Remove(message);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAll()
        {
            await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE dbo.messages");
        }

        public async Task<Boolean> Exists(string UUID)
        {
            return await _context.Messages.AnyAsync(m => m.UUID == UUID);
        }

        // Get all messages
        public async Task<IEnumerable<Message>> GetAll()
        {
            return await _context.Messages.ToListAsync();
        }

        public async Task<IEnumerable<Message>> GetAllFromChat(int chatId)
        {
            return await _context.Messages.Where(m => m.ChatId == chatId).ToListAsync();
        }

        public async Task<IEnumerable<Message>> GetFromChatSince(int chatId, string lastMessageUUID)
        {
            var lastSeenMessage = _context.Messages.FindAsync(lastMessageUUID);
            return await _context.Messages.Where(m => m.ChatId == chatId &&
                        m.Timestamp > lastSeenMessage.Result.Timestamp).ToListAsync();
        }

        public async Task Update(Message message)
        {
            _context.Entry(message).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}
