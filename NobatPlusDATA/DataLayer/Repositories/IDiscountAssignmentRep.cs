using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IDiscountAssignmentRep
    {
        public Task<ListResultObject<DiscountAssignment>> GetAllDiscountAssignmentsAsync(long DiscountId, long AdminId = 0, long StylistId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="");
        public Task<RowResultObject<DiscountAssignment>> GetDiscountAssignmentByIdAsync(long DiscountAssignmentId);
        public Task<BitResultObject> AddDiscountAssignmentsAsync(List<DiscountAssignment> DiscountAssignments);
        public Task<BitResultObject> EditDiscountAssignmentsAsync(List<DiscountAssignment> DiscountAssignments);
        public Task<BitResultObject> RemoveDiscountAssignmentsAsync(List<DiscountAssignment> discountAssignments);
        public Task<BitResultObject> RemoveDiscountAssignmentsAsync(List<long> discountAssignmentIds);
        public Task<BitResultObject> ExistDiscountAssignmentAsync(long DiscountAssignmentId);
    }
}
