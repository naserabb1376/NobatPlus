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
        public List<Review> GetAllReviews(long BookingId = 0,long CustomerId = 0 ,int pageIndex = 1,int pageSize = 20, string searchText ="");
        public Review GetReviewById(long ReviewId);
        public void AddReview(Review Review);
        public void EditReview(Review Review);
        public void RemoveReview(Review Review);
        public void RemoveReview(long ReviewId);
        public bool ExistReview(long ReviewId);
    }
}
