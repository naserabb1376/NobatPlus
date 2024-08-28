using Domain;
using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
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

        public async Task<BitResultObject> AddReviewAsync(Review Review)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                await _context.Reviews.AddAsync(Review);
                await _context.SaveChangesAsync();
                result.ID = Review.ID;
                _context.Entry(Review).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<BitResultObject> EditReviewAsync(Review Review)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.Reviews.Update(Review);
                await _context.SaveChangesAsync();
                result.ID = Review.ID;
                _context.Entry(Review).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<BitResultObject> ExistReviewAsync(long ReviewId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                result.Status = await _context.Reviews
                .AsNoTracking()
                .AnyAsync(x => x.ID == ReviewId);
                result.ID = ReviewId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<ListResultObject<Review>> GetAllReviewsAsync(long BookingId = 0, long CustomerId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            ListResultObject<Review> results = new ListResultObject<Review>();
            try
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
                    (!string.IsNullOrEmpty(x.ReviewDate.ToString()) && x.ReviewDate.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Comments) && x.Comments.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.LikeCount.ToString()) && x.LikeCount.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.DislikeCount.ToString()) && x.DislikeCount.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Rating.ToString()) && x.Rating.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Booking.BookingDate.ToString()) && x.Booking.BookingDate.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Booking.BookingTime.ToString()) && x.Booking.BookingTime.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Booking.Status.ToString()) && x.Booking.Status.ToString().Contains(searchText)) ||
                    (x.CreateDate.HasValue && x.CreateDate.Value.ToString().Contains(searchText)) ||
                    (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString().Contains(searchText))
                );
                results.TotalCount = query.Count();
                results.PageCount = DbTools.GetPageCount(results.TotalCount, pageSize);
                results.Results = await query.OrderByDescending(x => x.CreateDate)
                .ToPaging(pageIndex, pageSize)
                .ToListAsync();
            }
            catch (Exception ex)
            {
                results.Status = false;
                results.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return results;
       
        }

        public async Task<RowResultObject<Review>> GetReviewByIdAsync(long ReviewId)
        {
            RowResultObject<Review> result = new RowResultObject<Review>();
            try
            {
                result.Result = await _context.Reviews
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.ID == ReviewId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
           
        }

        public async Task<BitResultObject> RemoveReviewAsync(Review Review)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.Reviews.Remove(Review);
                await _context.SaveChangesAsync();
                result.ID = Review.ID;
                _context.Entry(Review).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<BitResultObject> RemoveReviewAsync(long ReviewId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var Review = await GetReviewByIdAsync(ReviewId);
                result = await RemoveReviewAsync(Review.Result);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }
    }
}
