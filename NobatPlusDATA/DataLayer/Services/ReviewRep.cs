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
    public class ReviewRep : IReviewRep
    {

        private NobatPlusContext _context;
        public ReviewRep()
        {
            _context = DbTools.GetDbContext();
        }

        public void AddReview(Review Review)
        {
            _context.Reviews.Add(Review);
            _context.SaveChanges();
            _context.Entry(Review).State = EntityState.Detached;
        }

        public void EditReview(Review Review)
        {
            _context.Reviews.Update(Review);
            _context.SaveChanges();
            _context.Entry(Review).State = EntityState.Detached;
        }

        public bool ExistReview(long ReviewId)
        {
            return _context.Reviews.Any(x => x.ID == ReviewId);
        }

        public List<Review> GetAllReviews(long BookingId = 0,long CustomerId = 0 ,int pageIndex= 1, int pageSize = 20, string searchText= "")
        {
            if (BookingId > 0)
            {
            return _context.Reviews.Include(x=> x.Booking).ThenInclude(x=> x.Stylist).Include(x=> x.Customer).ThenInclude(x => x.Person).Where(x =>
             (x.BookingID == BookingId) &&
             ((!string.IsNullOrEmpty(x.Customer.Person.FirstName.ToString()) && x.Customer.Person.FirstName.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Customer.Person.LastName.ToString()) && x.Customer.Person.LastName.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.ReviewDate.ToString()) && x.ReviewDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
            || (!string.IsNullOrEmpty(x.Comments.ToString()) && x.Comments.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.LikeCount.ToString()) && x.LikeCount.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.DislikeCount.ToString()) && x.DislikeCount.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Rating.ToString()) && x.Rating.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Booking.BookingDate.ToString()) && x.Booking.BookingDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
            || (!string.IsNullOrEmpty(x.Booking.BookingTime.ToString()) && x.Booking.BookingTime.ToString("HH:mm").Contains(searchText))
            || (!string.IsNullOrEmpty(x.Booking.Status.ToString()) && x.Booking.Status.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
            || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
             )).OrderByDescending(x=> x.CreateDate).ToPaging(pageIndex, pageSize).ToList();
            }

            if (CustomerId > 0)
            {
                return _context.Reviews.Include(x => x.Booking).ThenInclude(x => x.Stylist).Include(x => x.Customer).ThenInclude(x => x.Person).Where(x =>
                (x.CustomerID == CustomerId) &&
                 ((!string.IsNullOrEmpty(x.Customer.Person.FirstName.ToString()) && x.Customer.Person.FirstName.ToString().Contains(searchText))
                || (!string.IsNullOrEmpty(x.Customer.Person.LastName.ToString()) && x.Customer.Person.LastName.ToString().Contains(searchText))
                || (!string.IsNullOrEmpty(x.ReviewDate.ToString()) && x.ReviewDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                || (!string.IsNullOrEmpty(x.Comments.ToString()) && x.Comments.ToString().Contains(searchText))
                || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
                || (!string.IsNullOrEmpty(x.LikeCount.ToString()) && x.LikeCount.ToString().Contains(searchText))
                || (!string.IsNullOrEmpty(x.DislikeCount.ToString()) && x.DislikeCount.ToString().Contains(searchText))
                || (!string.IsNullOrEmpty(x.Rating.ToString()) && x.Rating.ToString().Contains(searchText))
                || (!string.IsNullOrEmpty(x.Booking.BookingDate.ToString()) && x.Booking.BookingDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                || (!string.IsNullOrEmpty(x.Booking.BookingTime.ToString()) && x.Booking.BookingTime.ToString("HH:mm").Contains(searchText))
                || (!string.IsNullOrEmpty(x.Booking.Status.ToString()) && x.Booking.Status.ToString().Contains(searchText))
                || (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                 )).OrderByDescending(x => x.CreateDate).ToPaging(pageIndex, pageSize).ToList();
            }
            else
            {
                return _context.Reviews.Include(x => x.Booking).ThenInclude(x => x.Stylist).Include(x => x.Customer).ThenInclude(x => x.Person).Where(x =>
                (!string.IsNullOrEmpty(x.Customer.Person.FirstName.ToString()) && x.Customer.Person.FirstName.ToString().Contains(searchText))
               || (!string.IsNullOrEmpty(x.Customer.Person.LastName.ToString()) && x.Customer.Person.LastName.ToString().Contains(searchText))
               || (!string.IsNullOrEmpty(x.ReviewDate.ToString()) && x.ReviewDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
               || (!string.IsNullOrEmpty(x.Comments.ToString()) && x.Comments.ToString().Contains(searchText))
               || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
               || (!string.IsNullOrEmpty(x.LikeCount.ToString()) && x.LikeCount.ToString().Contains(searchText))
               || (!string.IsNullOrEmpty(x.DislikeCount.ToString()) && x.DislikeCount.ToString().Contains(searchText))
               || (!string.IsNullOrEmpty(x.Rating.ToString()) && x.Rating.ToString().Contains(searchText))
               || (!string.IsNullOrEmpty(x.Booking.BookingDate.ToString()) && x.Booking.BookingDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
               || (!string.IsNullOrEmpty(x.Booking.BookingTime.ToString()) && x.Booking.BookingTime.ToString("HH:mm").Contains(searchText))
               || (!string.IsNullOrEmpty(x.Booking.Status.ToString()) && x.Booking.Status.ToString().Contains(searchText))
               || (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
               || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                ).OrderByDescending(x => x.CreateDate).ToPaging(pageIndex, pageSize).ToList();
            }
        }

        public Review GetReviewById(long ReviewId)
        {
            return _context.Reviews.Find(ReviewId);
        }

        public void RemoveReview(Review Review)
        {
            _context.Reviews.Remove(Review);
            _context.SaveChanges();
            _context.Entry(Review).State = EntityState.Detached;
        }

        public void RemoveReview(long ReviewId)
        {
            var Review = GetReviewById(ReviewId);
            RemoveReview(Review);
        }
    }
}
