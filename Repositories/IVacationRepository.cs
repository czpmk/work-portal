using WorkPortalAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkPortalAPI.Repositories
{
    public interface IVacationRepository
    {
        Task<List<Vacation>> Get();
        Task<Vacation> Get(int id);
        Task<List<Vacation>> GetByUserId(int userId);
        Task<List<Vacation>> GetByCompanyId(int companyId);
        Task<List<Vacation>> GetByDepartmentId(int departmentId);
        Task<Vacation> Create(Vacation vacation);
        Task Update(Vacation vacation);
        Task Delete(int id);
        Task DeleteAll();
    }
}
