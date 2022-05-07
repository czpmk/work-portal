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

        public async Task<List<Chat>> Get()
        {
            return await _context.Chats.ToListAsync();
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
            if (IsPrivateChat(chat) && await PrivateChatExists(chat.FirstUserId.GetValueOrDefault(), chat.SecondUserId.GetValueOrDefault()))
            {
                chatList.Add(await GetPrivateChat(chat.FirstUserId.GetValueOrDefault(),
                    chat.SecondUserId.GetValueOrDefault()));
            }
            else if (IsGroupChat(chat) && await GroupChatExists(chat.CompanyId.GetValueOrDefault(), chat.DepartamentId))
            {
                if (chat.DepartamentId == null)
                    chatList.Add(await GetCompanyChat(chat.CompanyId.GetValueOrDefault()));
                else
                    chatList.Add(await GetDepartamentChat(chat.CompanyId.GetValueOrDefault(),
                    chat.DepartamentId.GetValueOrDefault()));
            }

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

        public async Task<List<Message>> GetMessagesSince(int chatId, string UUID, int n = 50)
        {
            var message = await _context.Messages.FindAsync(UUID);
            var newerMessages = await _context.Messages.Where(m => m.ChatId == chatId && m.Timestamp > message.Timestamp).ToListAsync();
            // sort desc
            newerMessages.Sort((f, s) => s.Timestamp.CompareTo(s.Timestamp));
            if (n >= newerMessages.Count())
                return newerMessages;
            else
                return newerMessages.TakeLast(n).ToList();
        }

        public async Task<List<Message>> GetMessagesUntil(int chatId, string UUID, int n = 50)
        {
            var message = await _context.Messages.FindAsync(UUID);
            var olderMessages = await _context.Messages.Where(m => m.ChatId == chatId && m.Timestamp < message.Timestamp).ToListAsync();
            // sort desc
            olderMessages.Sort((f, s) => s.Timestamp.CompareTo(s.Timestamp));
            if (n >= olderMessages.Count())
                return olderMessages;
            else
                return olderMessages.Take(n).ToList();
        }

        public async Task<List<Message>> GetMessagesInRange(int chatId, string startRangeUUID, string endRangeUUID)
        {
            var startRangeMessage = await _context.Messages.FindAsync(startRangeUUID);
            var endRangeMessage = await _context.Messages.FindAsync(endRangeUUID);
            var messages = await _context.Messages.Where(m => m.ChatId == chatId &&
                                                                m.Timestamp > startRangeMessage.Timestamp &&
                                                                m.Timestamp < endRangeMessage.Timestamp
                                                                ).ToListAsync();
            // sort desc
            messages.Sort((f, s) => s.Timestamp.CompareTo(s.Timestamp));
            return messages;
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

        public async Task<Boolean> MessageExistsInChat(int chatId, string messageUUID)
        {
            return await _context.Messages.Where(m => m.ChatId == chatId && m.UUID == messageUUID).AnyAsync();
        }

        public async Task<Boolean> HasMessageOlderThan(int chatId, string messageUUID)
        {
            var message = await _context.Messages.FindAsync(messageUUID);
            return await _context.Messages.Where(m => m.ChatId == chatId && m.Timestamp < message.Timestamp).AnyAsync();
        }

        public async Task<Boolean> PrivateChatExists(int id_1, int id_2)
        {
            return await _context.Chats.Where(c => (c.FirstUserId == id_1 || c.FirstUserId == id_2) && (c.SecondUserId == id_1 || c.SecondUserId == id_2)).AnyAsync();
        }

        public async Task<Boolean> GroupChatExists(int companyId, int? departamentId = null)
        {
            return await _context.Chats.Where(c => (c.CompanyId == companyId) && (c.DepartamentId == departamentId)).AnyAsync();
        }

        public async Task<Dictionary<string, object>> GetChatDescriptionDictionary(int chatId)
        {
            var chat = await _context.Chats.FindAsync(chatId);
            var chatDescription = new Dictionary<string, object>();

            chatDescription.Add("users", null);
            chatDescription.Add("company", null);
            chatDescription.Add("departament", null);

            var usersRolesJoinedQuery = _context.Users.Join(
                _context.Roles,
                user => user.Id,
                role => role.UserId,
                (user, role) => new
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    Surname = user.Surname,
                    Email = user.Email,
                    CompanyId = role.CompanyId,
                    DepartamentId = role.DepartamentId
                }
                );


            if (await IsPrivateChat(chatId))
            {
                chatDescription["users"] = await usersRolesJoinedQuery.Where(u => u.Id == chat.FirstUserId || u.Id == chat.SecondUserId)
                    .ToDictionaryAsync(u => u.Id);
            }
            else
            {
                var company = await _context.Companies.FindAsync(chat.CompanyId);
                chatDescription.Add("company", company.Name);

                // departament = null => get users for an entire Company
                if (chat.DepartamentId == null)
                {
                    chatDescription["users"] = await usersRolesJoinedQuery.Where(u => chat.CompanyId == u.CompanyId)
                        .ToDictionaryAsync(u => u.Id);
                }
                // departament != null => get users for the departament only
                else
                {
                    var departament = await _context.Departaments.FindAsync(chat.DepartamentId);
                    chatDescription.Add("departament", departament.Name);

                    chatDescription["users"] = await usersRolesJoinedQuery.Where(u => u.CompanyId == chat.CompanyId &&
                                                                     u.DepartamentId == chat.DepartamentId).ToDictionaryAsync(u => u.Id);
                }
            }
            return chatDescription;
        }

        public async Task DeleteAll()
        {
            await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE dbo.chats");
        }
    }
}
