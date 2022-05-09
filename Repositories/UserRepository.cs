using WorkPortalAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WorkPortalAPI.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly WPContext _context;
        public UserRepository(WPContext context)
        {
            this._context = context;
        }
        public async Task<User> Create(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task Delete(int id)
        {
            var chatViewReports = await _context.ChatViewReports.Where(cvr => cvr.UserId == id).ToListAsync();
            foreach (var cvr in chatViewReports)
            {
                _context.ChatViewReports.Remove(cvr);
            }

            var chats = await _context.Chats.Where(c => c.FirstUserId == id || c.SecondUserId == id).ToListAsync();
            foreach (var c in chats)
            {
                _context.Chats.Remove(c);
            }

            var messages = await _context.Messages.Where(m => m.UserId == id).ToListAsync();
            foreach (var m in messages)
            {
                _context.Messages.Remove(m);
            }

            var role = await _context.Roles.Where(r => r.UserId == id).FirstAsync();
            _context.Roles.Remove(role);

            var sessions = await _context.Sessions.Where(s => s.UserId == id).ToListAsync();
            foreach (var s in sessions)
            {
                _context.Sessions.Remove(s);
            }

            var statuses = await _context.Statuses.Where(st => st.UserId == id).ToListAsync();
            foreach (var st in statuses)
            {
                _context.Statuses.Remove(st);
            }

            var vacations = await _context.Vacations.Where(v => v.UserId == id).ToListAsync();
            foreach (var v in vacations)
            {
                _context.Vacations.Remove(v);
            }

            var user = await _context.Users.FindAsync(id);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<User>> Get()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User> Get(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task Update(User user)
        {
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<Boolean> Exists(int id)
        {
            return await _context.Users.Where(u => u.Id == id).AnyAsync();
        }

        public async Task<Boolean> Exists(string email)
        {
            return await _context.Users.Where(u => u.Email == email).AnyAsync();
        }

        public async Task DeleteAll()
        {
            await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE dbo.users");
        }

        public async Task<IEnumerable<dynamic>> FindUsers(string? userNameNullable, int? companyIdNullable, int? departamentIdNullable)
        {
            var skipFilterByUsername = userNameNullable == null;
            var skipFilterByCompany = companyIdNullable == null;
            var skipFilterByDepartament = departamentIdNullable == null;

            var userNameList = new List<string>();
            if (!skipFilterByUsername)
            {
                userNameList = userNameNullable.Split(' ').Select(x => x.Trim().ToLower()).Where(x => x.Length != 0).ToList();
                // do not filter by user name if the arguments list is empty (e.g. whitespaces provided)
                skipFilterByUsername = userNameList.Count() == 0;
            }

            //var userRolesJoined = _context.Users.Join(
            //                                        _context.Roles,
            //                                        user => user.Id,
            //                                        role => role.UserId,
            //                                        (user, role) => new
            //                                        {
            //                                            Id = user.Id,
            //                                            FirstName = user.FirstName,
            //                                            Surname = user.Surname,
            //                                            Email = user.Email,
            //                                            CompanyId = role.Id,
            //                                            DepartamentId = role.Id,
            //                                        }
            //                                    );

            //var results =
            //    from u in _context.Users
            //    join r in _context.Roles on u.Id equals r.UserId
            //    select new
            //    {
            //        Id = u.Id,
            //        FirstName = u.FirstName,
            //        Surname = u.Surname,
            //        Email = u.Email,
            //        CompanyId = r.Id,
            //        DepartamentId = r.Id,
            //    };

            var results =
                from u in _context.Users
                join r in _context.Roles on u.Id equals r.UserId
                join c in _context.Companies on r.CompanyId equals c.Id
                join d in _context.Departaments on r.DepartamentId equals d.Id
                select new
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    Surname = u.Surname,
                    Email = u.Email,
                    CompanyId = c.Id,
                    CompanyName = c.Name,
                    DepartamentId = d.Id,
                    DepartamentName = d.Name
                };

            //var results = userRolesJoined;

            if (!skipFilterByCompany)
                results = results.Where(u => u.CompanyId == companyIdNullable);

            if (!skipFilterByDepartament)
                results = results.Where(u => u.DepartamentId == departamentIdNullable);

            if (skipFilterByUsername)
                return await results.ToListAsync<dynamic>();

            else
                return (await results.ToListAsync<dynamic>()).Where(r =>
                                        userNameList.All(x => r.FirstName.ToString().ToLower().Contains(x) ||
                                                              r.Surname.ToString().ToLower().Contains(x)));
        }
    }
}
