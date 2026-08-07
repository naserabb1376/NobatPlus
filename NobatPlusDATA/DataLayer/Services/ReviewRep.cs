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
        public ReviewRep(NobatPlusContext context)
        {
            _context = context;
        }

        public async Task<BitResultObject> AcceptReviewAsync(long ReviewId, long RoleId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var Review = await GetReviewByIdAsync(ReviewId, RoleId);

                Review.Result.IsAccepted = true;

                var theReview = new Review()
                {
                    CreateDate = Review.Result.CreateDate,
                    UpdateDate = Review.Result.UpdateDate,
                    Description = Review.Result.Description,
                    ID = Review.Result.ID,

                    StylistID = Review.Result.StylistID,
                    CustomerID = Review.Result.CustomerID,
                    BookingID = Review.Result.BookingID,

                    Comments = Review.Result.Comments,
                    ReviewDate = Review.Result.ReviewDate,
                    Rating = Review.Result.Rating,
                    Status = Review.Result.Status,
                    LikeCount = Review.Result.LikeCount,
                    DislikeCount = Review.Result.DislikeCount,
                    IsPrivate = Review.Result.IsPrivate,
                    IsAccepted = Review.Result.IsAccepted,
                };

                result = await EditReviewAsync(theReview);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
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

        public async Task<ListResultObject<ReviewDto>> GetAllReviewsAsync(
     long RoleId,
     long BookingId = 0,
     long CustomerId = 0,
     long StylistId = 0,
     int pageIndex = 1,
     int pageSize = 20,
     string searchText = "",
     string sortQuery = "")
        {
            ListResultObject<ReviewDto> results =
                new ListResultObject<ReviewDto>();

            try
            {
                IQueryable<Review> query = _context.Reviews
                    .AsNoTracking()
                    .Include(x => x.Booking)
                        .ThenInclude(x => x.Stylist)
                        .ThenInclude(x => x.Person)
                    .Include(x => x.Customer)
                        .ThenInclude(x => x.Person);


                // =========================================
                // Access Control
                // =========================================

                if (RoleId > 0 && RoleId != 4)
                {
                    query = query.Where(x => x.IsAccepted);
                }


                // =========================================
                // Filters
                // =========================================

                if (BookingId > 0)
                {
                    query = query.Where(x => x.BookingID == BookingId);
                }

                if (StylistId > 0)
                {
                    query = query.Where(x => x.StylistID == StylistId);
                }
                else if (CustomerId > 0)
                {
                    query = query.Where(x => x.CustomerID == CustomerId);
                }


                // =========================================
                // Search
                // =========================================

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    query = query.Where(x =>
                        (!string.IsNullOrEmpty(x.Customer.Person.FirstName) &&
                         x.Customer.Person.FirstName.Contains(searchText)) ||

                        (!string.IsNullOrEmpty(x.Customer.Person.LastName) &&
                         x.Customer.Person.LastName.Contains(searchText)) ||

                        x.ReviewDate.ToString().Contains(searchText) ||

                        (!string.IsNullOrEmpty(x.Comments) &&
                         x.Comments.Contains(searchText)) ||

                        (!string.IsNullOrEmpty(x.Description) &&
                         x.Description.Contains(searchText)) ||

                        x.LikeCount.ToString().Contains(searchText) ||

                        x.DislikeCount.ToString().Contains(searchText) ||

                        x.Rating.ToString().Contains(searchText) ||

                        x.Booking.BookingDate.ToString().Contains(searchText) ||

                        x.Booking.Status.Contains(searchText) ||

                        (x.CreateDate.HasValue &&
                         x.CreateDate.Value.ToString().Contains(searchText)) ||

                        (x.UpdateDate.HasValue &&
                         x.UpdateDate.Value.ToString().Contains(searchText))
                    );
                }


                // =========================================
                // Count
                // =========================================

                results.TotalCount = await query.CountAsync();

                results.PageCount =
                    DbTools.GetPageCount(
                        results.TotalCount,
                        pageSize);


                // =========================================
                // Get Current Page
                // =========================================

                var reviews = await query
                    .OrderByDescending(x => x.CreateDate)
                    .SortBy(sortQuery)
                    .ToPaging(pageIndex, pageSize)
                    .ToListAsync();


                // =========================================
                // Get RateHistory averages
                // =========================================

                if (reviews.Any())
                {
                    var bookingIds = reviews
                        .Select(x => x.BookingID)
                        .Distinct()
                        .ToList();

                    var customerIds = reviews
                        .Select(x => x.CustomerID)
                        .Distinct()
                        .ToList();

                    var stylistIds = reviews
                        .Select(x => x.StylistID)
                        .Distinct()
                        .ToList();


                    var rateAverages = await _context.RateHistories
                        .AsNoTracking()
                        .Where(x =>
                            bookingIds.Contains(x.BookingID) &&
                            customerIds.Contains(x.CustomerID) &&
                            stylistIds.Contains(x.StylistID))
                        .GroupBy(x => new
                        {
                            x.BookingID,
                            x.CustomerID,
                            x.StylistID
                        })
                        .Select(g => new
                        {
                            g.Key.BookingID,
                            g.Key.CustomerID,
                            g.Key.StylistID,

                            AverageRateScore = g.Average(x => x.RateScore)
                        })
                        .ToListAsync();


                    // تبدیل به Dictionary برای دسترسی سریع
                    var rateAverageDictionary = rateAverages
                        .ToDictionary(
                            x => (
                                x.BookingID,
                                x.CustomerID,
                                x.StylistID
                            ),
                            x => x.AverageRateScore
                        );


                    // =========================================
                    // Map Review -> ReviewDto
                    // =========================================

                    results.Results = reviews
                        .Select(review =>
                        {
                            var key = (
                                review.BookingID,
                                review.CustomerID,
                                review.StylistID
                            );

                            rateAverageDictionary.TryGetValue(
                                key,
                                out var averageRateScore);

                            return new ReviewDto
                            {
                                ID = review.ID,

                                BookingID = review.BookingID,
                                CustomerID = review.CustomerID,
                                StylistID = review.StylistID,

                                Rating = review.Rating,
                                Comments = review.Comments,
                                Status = review.Status,

                                LikeCount = review.LikeCount,
                                DislikeCount = review.DislikeCount,

                                IsPrivate = review.IsPrivate,
                                IsAccepted = review.IsAccepted,

                                ReviewDate = review.ReviewDate,

                                AverageRateScore = averageRateScore,

                                CreateDate = review.CreateDate,
                                UpdateDate = review.UpdateDate,
                                Description = review.Description,

                                StylistName = review.Booking.Stylist.Person.FirstName + " " + review.Booking.Stylist.Person.LastName,
                                CustomerName = (!review.IsPrivate) ? review.Customer.Person.FirstName + " " + review.Customer.Person.LastName : "ناشناس",
                                SalonName = review.Booking.Stylist.StylistName,
                            };
                        })
                        .ToList();
                }
                else
                {
                    results.Results = new List<ReviewDto>();
                }
            }
            catch (Exception ex)
            {
                results.Status = false;

                results.ErrorMessage =
                    $"{ex.Message} - {ex.InnerException?.Message}";
            }

            return results;
        }

        public async Task<RowResultObject<ReviewDto>> GetReviewByIdAsync(
      long ReviewId,
      long RoleId)
        {
            RowResultObject<ReviewDto> result =
                new RowResultObject<ReviewDto>();

            try
            {
                // =========================================
                // Get Review
                // =========================================

                var review = await _context.Reviews
                    .AsNoTracking()
                    .Include(x => x.Booking)
                        .ThenInclude(x => x.Stylist)
                        .ThenInclude(x => x.Person)
                    .Include(x => x.Customer)
                        .ThenInclude(x => x.Person)
                    .SingleOrDefaultAsync(x => x.ID == ReviewId);


                // =========================================
                // Check Not Found
                // =========================================

                if (review == null)
                {
                    throw new Exception("Review مورد نظر پیدا نشد");
                }


                // =========================================
                // Access Control
                // =========================================

                if ((RoleId > 0 && RoleId != 4) &&
                    !review.IsAccepted)
                {
                    throw new Exception(
                        "شما دسترسی لازم برای انجام این عملیات را ندارید");
                }


                // =========================================
                // Calculate RateHistory Average
                // =========================================

                var averageRateScore =
                    await _context.RateHistories
                        .AsNoTracking()
                        .Where(x =>
                            x.BookingID == review.BookingID &&
                            x.CustomerID == review.CustomerID &&
                            x.StylistID == review.StylistID)
                        .Select(x => (float?)x.RateScore)
                        .AverageAsync() ?? 0;


                // =========================================
                // Map Review -> ReviewDto
                // =========================================

                result.Result = new ReviewDto
                {
                    ID = review.ID,

                    BookingID = review.BookingID,
                    CustomerID = review.CustomerID,
                    StylistID = review.StylistID,

                    Rating = review.Rating,
                    Comments = review.Comments,
                    Status = review.Status,

                    LikeCount = review.LikeCount,
                    DislikeCount = review.DislikeCount,

                    IsPrivate = review.IsPrivate,
                    IsAccepted = review.IsAccepted,

                    ReviewDate = review.ReviewDate,

                    AverageRateScore = averageRateScore,

                    CreateDate = review.CreateDate,
                    UpdateDate = review.UpdateDate,
                    Description = review.Description,

                    StylistName = review.Booking.Stylist.Person.FirstName + " " + review.Booking.Stylist.Person.LastName,
                    CustomerName = (!review.IsPrivate) ? review.Customer.Person.FirstName + " " + review.Customer.Person.LastName : "ناشناس",
                    SalonName = review.Booking.Stylist.StylistName,
                };
            }
            catch (Exception ex)
            {
                result.Status = false;

                result.ErrorMessage =
                    $"{ex.Message} - {ex.InnerException?.Message}";
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

        public async Task<BitResultObject> RemoveReviewAsync(long ReviewId, long RoleId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var Review = await GetReviewByIdAsync(ReviewId, RoleId);
                var theReview = new Review()
                {
                    CreateDate = Review.Result.CreateDate,
                    UpdateDate = Review.Result.UpdateDate,
                    Description = Review.Result.Description,
                    ID = Review.Result.ID,

                    StylistID = Review.Result.StylistID,
                    CustomerID = Review.Result.CustomerID,
                    BookingID = Review.Result.BookingID,

                    Comments = Review.Result.Comments,
                    ReviewDate = Review.Result.ReviewDate,
                    Rating = Review.Result.Rating,
                    Status = Review.Result.Status,
                    LikeCount = Review.Result.LikeCount,
                    DislikeCount = Review.Result.DislikeCount,
                    IsPrivate = Review.Result.IsPrivate,
                    IsAccepted = Review.Result.IsAccepted,
                };
                result = await RemoveReviewAsync(theReview);
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
