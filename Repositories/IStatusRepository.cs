using WorkPortalAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkPortalAPI.Repositories
{
    public interface IStatusRepository
    {
        Task<IEnumerable<Status>> Get();
        Task<Status> Get(int id);
        Task<Status> Create(Status status);
        Task Update(Status status);
        Task Delete(int id);
        Task Delete(string token);
    }
}
