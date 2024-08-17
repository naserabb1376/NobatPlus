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

        public async Task AddReviewAsync(Review Review)
        {
            _context.Reviews.Add(Review);
            await _context.SaveChangesAsync();
            _context.Entry(Review).State = EntityState.Detached;
        }

        public async Task EditReviewAsync(Review Review)
        {
            _context.Reviews.Update(Review);
            await _context.SaveChangesAsync();
            _context.Entry(Review).State = EntityState.Detached;
        }

        public async Task<bool> ExistReviewAsync(long ReviewId)
        {
            return await _context.Reviews
                .AsNoTracking()
                .AnyAsync(x => x.ID == ReviewId);
        }

        public async Task<List<Review>> GetAllReviewsAsync(long BookingId = 0, long CustomerId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            IQueryable<Review> query = _context.Reviews
                .AsNoTracking()
                .Include(x => x.Booking).ThenInclude(x => x.Stylist)
                .Include(x => x.Customer).ThenInclude(x => x.Person);

            if (BookingId > 0)
            {
                query = query.Where(x => x.BookingID == BookingId);
            }
            else if (CustomerId > 0)
            {
                query = query.Where(x => x.CustomerID == CustomerId);
            }

            query = query.Where(x =>
                (!string.IsNullOrEmpty(x.Customer.Person.FirstName) && x.Customer.Person.FirstName.Contains(searchText)) ||
                (!string.IsNullOrEmpty(x.Customer.Person.LastName) && x.Customer.Person.LastName.Contains(searchText)) ||
                (!string.IsNullOrEmpty(x.ReviewDate.ToString()) && x.ReviewDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                (!string.IsNullOrEmpty(x.Comments) && x.Comments.Contains(searchText)) ||
                (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText)) ||
                (!string.IsNullOrEmpty(x.LikeCount.ToString()) && x.LikeCount.ToString().Contains(searchText)) ||
                (!string.IsNullOrEmpty(x.DislikeCount.ToString()) && x.DislikeCount.ToString().Contains(searchText)) ||
                (!string.IsNullOrEmpty(x.Rating.ToString()) && x.Rating.ToString().Contains(searchText)) ||
                (!string.IsNullOrEmpty(x.Booking.BookingDate.ToString()) && x.Booking.BookingDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                (!string.IsNullOrEmpty(x.Booking.BookingTime.ToString()) && x.Booking.BookingTime.ToString("HH:mm").Contains(searchText)) ||
                (!string.IsNullOrEmpty(x.Booking.Status.ToString()) && x.Booking.Status.ToString().Contains(searchText)) ||
                (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
            );

            return await query
                .OrderByDescending(x => x.CreateDate)
                .ToPaging(pageIndex, pageSize)
                .ToListAsync();
        }

        public async Task<Review> GetReviewByIdAsync(long ReviewId)
        {
            return await _context.Reviews
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.ID == ReviewId);
        }

        public async Task RemoveReviewAsync(Review Review)
        {
            _context.Reviews.Remove(Review);
            await _context.SaveChangesAsync();
            _context.Entry(Review).State = EntityState.Detached;
        }

        public async Task RemoveReviewAsync(long ReviewId)
        {
            var Review = await GetReviewByIdAsync(ReviewId);
            await RemoveReviewAsync(Review);
        }
    }
}
