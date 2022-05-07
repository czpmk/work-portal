using WorkPortalAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkPortalAPI.Repositories
{
    public interface IVacationRepository
    {
        Task<IEnumerable<Vacation>> Get();
        Task<Vacation> Get(int id);
        Task<Vacation> Create(Vacation vacation);
        Task Update(Vacation vacation);
        Task Delete(int id);
        Task DeleteAll();
    }
}
