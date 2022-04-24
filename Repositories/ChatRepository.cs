using WorkPortalAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkPortalAPI.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly WPContext _context;
        public ChatRepository(WPContext context)
        {
            this._context = context;
        }
        public async Task<Chat> Create(Chat chat)
        {
            _context.Chats.Add(chat);
            await _context.SaveChangesAsync();
            return chat;
        }

        public async Task<Boolean> Exists(int id)
        {
            return await _context.Chats.AnyAsync(c => c.Id == id);
        }

        public async Task Delete(int id)
        {
            var chat = await _context.Chats.FindAsync(id);
            _context.Chats.Remove(chat);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Chat>> GetGroupChats()
        {
            return await _context.Chats.Where(c => c.FirstUserId == null && c.SecondUserId == null).ToListAsync();
        }

        public async Task<IEnumerable<Chat>> GetGroupChats(int companyId)
        {
            return await _context.Chats.Where(c => c.FirstUserId == null &&
                                                c.SecondUserId == null &&
                                                c.CompanyId == companyId
                                                ).ToListAsync();
        }

        public async Task<IEnumerable<Chat>> GetGroupChats(int companyId, int departamentId)
        {
            return await _context.Chats.Where(c => c.FirstUserId == null &&
                                                c.SecondUserId == null &&
                                                c.CompanyId == companyId &&
                                                c.DepartamentId == departamentId
                                                ).ToListAsync();
        }
        public async Task<Message> GetLastMessage(int chatId)
        {
            var messages = await _context.Messages.Where(m => m.ChatId == chatId).ToListAsync();
            messages.Sort((f, s) => s.Timestamp.CompareTo(f.Timestamp));
            return messages.FirstOrDefault();
        }

        public async Task<IEnumerable<Message>> GetMessages(int chatId, int n = -1)
        {
            var messages = await _context.Messages.Where(m => m.ChatId == chatId).ToListAsync();
            if (n == -1)
            {
                return messages;
            }
            else
            {
                messages.Sort((f, s) => s.Timestamp.CompareTo(f.Timestamp));
                return messages.Take(n);
            }
        }

        public async Task<IEnumerable<Message>> GetMessagesSince(int chatId, DateTime timestamp)
        {
            var messages = await _context.Messages.Where(m => m.ChatId == chatId &&
                                                              m.Timestamp > timestamp
                                                         ).ToListAsync();
            messages.Sort((f, s) => s.Timestamp.CompareTo(f.Timestamp));
            return messages;
        }

        public async Task<IEnumerable<Message>> GetMessagesSince(int chatId, Message lastMessage)
        {
            return await _context.Messages.Where(m => m.ChatId == chatId &&
                                                      m.Timestamp > lastMessage.Timestamp).ToListAsync();
        }

        public async Task<IEnumerable<Chat>> GetPrivateChats()
        {
            return await _context.Chats.Where(c => c.FirstUserId != null && c.SecondUserId != null).ToListAsync();
        }

        public async Task<IEnumerable<Chat>> GetPrivateChats(int userId)
        {
            return await _context.Chats.Where(c => c.FirstUserId == userId || c.SecondUserId == userId).ToListAsync();
        }

        public async Task<IEnumerable<Chat>> GetPrivateChats(int firstUserId, int secondUserId)
        {
            return await _context.Chats.Where(c => (firstUserId == c.FirstUserId || firstUserId == c.SecondUserId) &&
                                                   (secondUserId == c.FirstUserId || secondUserId == c.SecondUserId)
                                                   ).ToListAsync();
        }

        public async Task Update(Chat chat)
        {
            _context.Entry(chat).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}
