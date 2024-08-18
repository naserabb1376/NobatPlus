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
        public Task<ListResultObject<DiscountAssignment>> GetAllDiscountAssignmentsAsync(long DiscountId, long AdminId = 0, long StylistId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<RowResultObject<DiscountAssignment>> GetDiscountAssignmentByIdAsync(long DiscountAssignmentId);
        public Task<BitResultObject> AddDiscountAssignmentAsync(DiscountAssignment DiscountAssignment);
        public Task<BitResultObject> EditDiscountAssignmentAsync(DiscountAssignment DiscountAssignment);
        public Task<BitResultObject> RemoveDiscountAssignmentAsync(DiscountAssignment DiscountAssignment);
        public Task<BitResultObject> RemoveDiscountAssignmentAsync(long DiscountAssignmentId);
        public Task<BitResultObject> ExistDiscountAssignmentAsync(long DiscountAssignmentId);
    }
}
