using WorkPortalAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkPortalAPI.Repositories
{
    public interface IDepartamentRepository
    {
        Task<IEnumerable<Departament>> Get();
        Task<Departament> Get(int id);
        Task<IEnumerable<Departament>> GetByCompanyId(int companyId);
        Task<Departament> Create(Departament departament);
        Task Update(Departament departament);
        Task Delete(int id);
        Task<Boolean> Exists(int id);
        Task<Boolean> Exists(Departament departament);
        Task<User> GetOwner(Departament departament);
        Task<User> RetractOwnership(User user);
        Task<User> GrantOwnership(User user, RoleType type, int departamentId);
    }
}
