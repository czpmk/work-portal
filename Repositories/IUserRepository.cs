using WorkPortalAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkPortalAPI.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> Get();
        Task<User> Get(int id);
        Task<User> Create(User user);
        Task Update(User user);
        Task Delete(int id);
        Task<Boolean> Exists(int id);
        Task<Boolean> Exists(string email);
        Task DeleteAll();
        Task<IEnumerable<dynamic>> FindUsers(string? userNameNullable, int? companyIdNullable, int? departamentIdNullable);
        Task<IEnumerable<dynamic>> GetInfoForUsers(int? userId, int? companyId, int? departamentId);
    }
}
