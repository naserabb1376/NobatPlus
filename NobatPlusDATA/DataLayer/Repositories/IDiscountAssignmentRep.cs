using NobatPlusDATA.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IDiscountAssignmentRep
    {
        public Task<List<DiscountAssignment>> GetAllDiscountAssignmentsAsync(long DiscountId, long AdminId = 0, long StylistId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<DiscountAssignment> GetDiscountAssignmentByIdAsync(long DiscountAssignmentId);
        public Task AddDiscountAssignmentAsync(DiscountAssignment DiscountAssignment);
        public Task EditDiscountAssignmentAsync(DiscountAssignment DiscountAssignment);
        public Task RemoveDiscountAssignmentAsync(DiscountAssignment DiscountAssignment);
        public Task RemoveDiscountAssignmentAsync(long DiscountAssignmentId);
        public Task<bool> ExistDiscountAssignmentAsync(long DiscountAssignmentId);
    }
}
