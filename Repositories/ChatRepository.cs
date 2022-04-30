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

        public async Task<Chat> Get(int chatId)
        {
            return await _context.Chats.FindAsync(chatId);
        }

        public async Task<Boolean> Exists(int id)
        {
            return await _context.Chats.AnyAsync(c => c.Id == id);
        }

        public async Task<Boolean> Exists(Chat chat)
        {
            List<Chat> chatList = new List<Chat>();
            if (chat.FirstUserId != null && chat.SecondUserId != null)
                chatList.Add(await GetPrivateChat(chat.FirstUserId.GetValueOrDefault(),
                                    chat.SecondUserId.GetValueOrDefault()));

            else if (chat.CompanyId != null && chat.DepartamentId == null)
                chatList.Add(await GetCompanyChat(chat.CompanyId.GetValueOrDefault()));

            else if (chat.CompanyId != null && chat.DepartamentId != null)
                chatList.Add(await GetDepartamentChat(chat.CompanyId.GetValueOrDefault(),
                                    chat.DepartamentId.GetValueOrDefault()));

            return chatList.Any();
        }

        public async Task Delete(int id)
        {
            var chatViewReports = await _context.ChatViewReports.Where(c => c.ChatId == id).ToListAsync();
            foreach (var c in chatViewReports)
                _context.ChatViewReports.Remove(c);

            var messages = await _context.Messages.Where(m => m.ChatId == id).ToListAsync();
            foreach (var m in messages)
                _context.Messages.Remove(m);

            var chat = await _context.Chats.FindAsync(id);
            _context.Chats.Remove(chat);

            await _context.SaveChangesAsync();
        }

        public async Task<List<Chat>> GetGroupChats()
        {
            return await _context.Chats.Where(c => c.FirstUserId == null && c.SecondUserId == null).ToListAsync();
        }

        public async Task<Chat> GetCompanyChat(int companyId)
        {
            return await _context.Chats.Where(c => c.FirstUserId == null &&
                                                c.SecondUserId == null &&
                                                c.CompanyId == companyId &&
                                                c.DepartamentId == null
                                                ).FirstOrDefaultAsync();
        }

        public async Task<List<Chat>> GetDepartamentChats(int companyId)
        {
            return await _context.Chats.Where(c => c.FirstUserId == null &&
                                                c.SecondUserId == null &&
                                                c.CompanyId == companyId
                                                ).ToListAsync();
        }

        public async Task<Chat> GetDepartamentChat(int companyId, int departamentId)
        {
            return await _context.Chats.Where(c => c.FirstUserId == null &&
                                                c.SecondUserId == null &&
                                                c.CompanyId == companyId &&
                                                c.DepartamentId == departamentId
                                                ).FirstOrDefaultAsync();
        }
        public async Task<Message> GetLastMessage(int chatId)
        {
            var messages = await _context.Messages.Where(m => m.ChatId == chatId).ToListAsync();
            messages.Sort((f, s) => s.Timestamp.CompareTo(f.Timestamp));
            return messages.FirstOrDefault();
        }

        public async Task<List<Message>> GetMessages(int chatId, int n = -1)
        {
            var messages = await _context.Messages.Where(m => m.ChatId == chatId).ToListAsync();
            if (n == -1)
            {
                return messages;
            }
            else
            {
                messages.Sort((f, s) => s.Timestamp.CompareTo(f.Timestamp));
                return messages.Take(n).ToList();
            }
        }

        public async Task<List<Message>> GetMessagesSince(int chatId, DateTime timestamp)
        {
            var messages = await _context.Messages.Where(m => m.ChatId == chatId &&
                                                              m.Timestamp > timestamp
                                                         ).ToListAsync();
            messages.Sort((f, s) => s.Timestamp.CompareTo(f.Timestamp));
            return messages;
        }

        public async Task<List<Message>> GetMessagesSince(int chatId, Message lastMessage)
        {
            return await _context.Messages.Where(m => m.ChatId == chatId &&
                                                      m.Timestamp > lastMessage.Timestamp).ToListAsync();
        }

        public async Task<List<Chat>> GetPrivateChats()
        {
            return await _context.Chats.Where(c => c.FirstUserId != null && c.SecondUserId != null).ToListAsync();
        }

        public async Task<List<Chat>> GetPrivateChats(int userId)
        {
            return await _context.Chats.Where(c => c.FirstUserId == userId || c.SecondUserId == userId).ToListAsync();
        }

        public async Task<Chat> GetPrivateChat(int firstUserId, int secondUserId)
        {
            return await _context.Chats.Where(c => (firstUserId == c.FirstUserId || firstUserId == c.SecondUserId) &&
                                                   (secondUserId == c.FirstUserId || secondUserId == c.SecondUserId)
                                                   ).FirstOrDefaultAsync();
        }

        public async Task Update(Chat chat)
        {
            _context.Entry(chat).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public Boolean IsPrivateChat(Chat chat)
        {
            return chat.FirstUserId != null && chat.SecondUserId != null && chat.CompanyId == null && chat.DepartamentId == null;
        }
        public async Task<Boolean> IsPrivateChat(int chatId)
        {
            return IsPrivateChat(await Get(chatId));
        }

        public Boolean IsGroupChat(Chat chat)
        {
            return chat.FirstUserId == null && chat.SecondUserId == null && chat.CompanyId != null;
        }
        public async Task<Boolean> IsGroupChat(int chatId)
        {
            return IsGroupChat(await Get(chatId));
        }
    }
}
