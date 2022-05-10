using WorkPortalAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkPortalAPI.Repositories
{
    public interface IDepartmentRepository
    {
        Task<IEnumerable<Department>> Get();
        Task<Department> Get(int id);
        Task<IEnumerable<Department>> GetByCompanyId(int companyId);
        Task<Department> Create(Department department);
        Task Update(Department department);
        Task Delete(int id);
        Task<Boolean> Exists(int id);
        Task<Boolean> Exists(Department department);
        Task<User> GetOwner(Department department);
        Task<User> RetractOwnership(User user);
        Task<User> GrantOwnership(User user, RoleType type, int departmentId);
        Task DeleteAll();
    }
}
