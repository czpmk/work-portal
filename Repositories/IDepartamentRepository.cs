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
        Task<Departament> Create(Departament departament);
        Task Update(Departament departament);
        Task Delete(int id);
    }
}
