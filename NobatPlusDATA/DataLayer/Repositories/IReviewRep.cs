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
        public Task<ListResultObject<Review>> GetAllReviewsAsync(long BookingId = 0, long CustomerId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<RowResultObject<Review>> GetReviewByIdAsync(long ReviewId);
        public Task<BitResultObject> AddReviewAsync(Review Review);
        public Task<BitResultObject> EditReviewAsync(Review Review);
        public Task<BitResultObject> RemoveReviewAsync(Review Review);
        public Task<BitResultObject> RemoveReviewAsync(long ReviewId);
        public Task<BitResultObject> ExistReviewAsync(long ReviewId);
    }
}
