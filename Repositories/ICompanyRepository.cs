using WorkPortalAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkPortalAPI.Repositories
{
    public interface ICompanyRepository
    {
        Task<IEnumerable<Company>> Get();
        Task<Company> Get(int id);
        Task<Company> Create(Company company);
        Task Update(Company company);
        Task Delete(int id);
        Task<Boolean> Exists(int id);
        Task<Boolean> Exists(Company company);
    }
}
