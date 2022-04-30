using WorkPortalAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkPortalAPI.Repositories
{
    public interface IRoleRepository
    {
        Task<IEnumerable<Role>> Get();
        Task<Role> Get(int id);
        Task<Role> GetByUserId(int userId);
        Task<Role> Create(Role role);
        Task Update(Role role);
        Task Delete(int id);
    }
}
