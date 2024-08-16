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

        public void AddJobType(JobType JobType)
        {
            _context.JobTypes.Add(JobType);
            _context.SaveChanges();
            _context.Entry(JobType).State = EntityState.Detached;
        }

        public void EditJobType(JobType JobType)
        {
            _context.JobTypes.Update(JobType);
            _context.SaveChanges();
            _context.Entry(JobType).State = EntityState.Detached;
        }

        public bool ExistJobType(long JobTypeId)
        {
            return _context.JobTypes.Any(x => x.ID == JobTypeId);
        }

        public List<JobType> GetAllJobTypes(int SexTypeChecked = 0, int pageIndex= 1, int pageSize = 20, string searchText= "")
        {
            return _context.JobTypes.Where(x =>
            (x.SexTypeChecked == SexTypeChecked) &&
             ((!string.IsNullOrEmpty(x.JobTitle.ToString()) && x.JobTitle.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.SexTypeChecked.ToString()) && x.SexTypeChecked.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
            || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
             )).OrderByDescending(x=> x.CreateDate).ToPaging(pageIndex, pageSize).ToList();
        }

        public JobType GetJobTypeById(long JobTypeId)
        {
            return _context.JobTypes.Find(JobTypeId);
        }

        public void RemoveJobType(JobType JobType)
        {
            _context.JobTypes.Remove(JobType);
            _context.SaveChanges();
            _context.Entry(JobType).State = EntityState.Detached;
        }

        public void RemoveJobType(long JobTypeId)
        {
            var JobType = GetJobTypeById(JobTypeId);
            RemoveJobType(JobType);
        }
    }
}
