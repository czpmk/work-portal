using WorkPortalAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkPortalAPI.Repositories
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly WPContext _context;
        public CompanyRepository(WPContext context)
        {
            this._context = context;
        }

        public async Task<Company> Create(Company company)
        {
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();
            return company;
        }

        public async Task Delete(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Company>> Get()
        {
            return await _context.Companies.ToListAsync();
        }

        public async Task<Company> Get(int id)
        {
            return await _context.Companies.FindAsync(id);
        }

        public async Task Update(Company company)
        {
            _context.Entry(company).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<Boolean> Exists(int id)
        {
            return await _context.Companies.Where(c => c.Id == id).AnyAsync();
        }

        public async Task<Boolean> Exists(Company company)
        {
            return await _context.Companies.Where(c => c.Name == company.Name).AnyAsync();
        }
    }
}
