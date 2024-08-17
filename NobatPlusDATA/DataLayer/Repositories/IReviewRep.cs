using NobatPlusDATA.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IReviewRep
    {
        public Task<List<Review>> GetAllReviewsAsync(long BookingId = 0, long CustomerId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<Review> GetReviewByIdAsync(long ReviewId);
        public Task AddReviewAsync(Review Review);
        public Task EditReviewAsync(Review Review);
        public Task RemoveReviewAsync(Review Review);
        public Task RemoveReviewAsync(long ReviewId);
        public Task<bool> ExistReviewAsync(long ReviewId);
    }
}
