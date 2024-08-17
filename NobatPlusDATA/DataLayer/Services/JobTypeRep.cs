using Domain;
using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Services
{
    public class JobTypeRep : IJobTypeRep
    {

        private NobatPlusContext _context;
        public JobTypeRep()
        {
            _context = DbTools.GetDbContext();
        }

        public async Task AddJobTypeAsync(JobType JobType)
        {
            _context.JobTypes.Add(JobType);
            await _context.SaveChangesAsync();
            _context.Entry(JobType).State = EntityState.Detached;
        }

        public async Task EditJobTypeAsync(JobType JobType)
        {
            _context.JobTypes.Update(JobType);
            await _context.SaveChangesAsync();
            _context.Entry(JobType).State = EntityState.Detached;
        }

        public async Task<bool> ExistJobTypeAsync(long JobTypeId)
        {
            return await _context.JobTypes
                .AsNoTracking()
                .AnyAsync(x => x.ID == JobTypeId);
        }

        public async Task<List<JobType>> GetAllJobTypesAsync(int SexTypeChecked = 0, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            return await _context.JobTypes
                .AsNoTracking()
                .Where(x =>
                    x.SexTypeChecked == SexTypeChecked &&
                    (
                        (!string.IsNullOrEmpty(x.JobTitle.ToString()) && x.JobTitle.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.SexTypeChecked.ToString()) && x.SexTypeChecked.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                    )
                )
                .OrderByDescending(x => x.CreateDate)
                .ToPaging(pageIndex, pageSize)
                .ToListAsync();
        }

        public async Task<JobType> GetJobTypeByIdAsync(long JobTypeId)
        {
            return await _context.JobTypes
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.ID == JobTypeId);
        }

        public async Task RemoveJobTypeAsync(JobType JobType)
        {
            _context.JobTypes.Remove(JobType);
            await _context.SaveChangesAsync();
            _context.Entry(JobType).State = EntityState.Detached;
        }

        public async Task RemoveJobTypeAsync(long JobTypeId)
        {
            var JobType = await GetJobTypeByIdAsync(JobTypeId);
            await RemoveJobTypeAsync(JobType);
        }

    }
}
