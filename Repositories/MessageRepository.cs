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
        public async Task<Message> Create(Message message)
        {
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task Delete(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(string messageUUID)
        {
            var message = _context.Messages.FirstOrDefault(m => m.MessageUUID == messageUUID);
            if (message != null)
            {
                _context.Messages.Remove(message);
                await _context.SaveChangesAsync();
            }
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
                        lastSeenMessage.Result.Timestamp > m.Timestamp).ToListAsync();
        }

        public async Task Update(Message message)
        {
            var oldMessage = _context.Messages.FindAsync(message.Id);
            message.Id = oldMessage.Result.Id;
            message.MessageUUID = oldMessage.Result.MessageUUID;
            _context.Entry(message).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}
