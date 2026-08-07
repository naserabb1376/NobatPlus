using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IReviewRep
    {
        public Task<ListResultObject<ReviewDto>> GetAllReviewsAsync(long RoleId,long BookingId = 0, long CustomerId = 0, long StylistId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="");
        public Task<RowResultObject<ReviewDto>> GetReviewByIdAsync(long ReviewId, long RoleId);
        public Task<BitResultObject> AddReviewAsync(Review Review);
        public Task<BitResultObject> EditReviewAsync(Review Review);
        public Task<BitResultObject> RemoveReviewAsync(Review Review);
        public Task<BitResultObject> RemoveReviewAsync(long ReviewId, long RoleId);
        public Task<BitResultObject> AcceptReviewAsync(long ReviewId, long RoleId);
        public Task<BitResultObject> ExistReviewAsync(long ReviewId);
    }
}
