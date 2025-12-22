using Domain;
using Domains;
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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NobatPlusDATA.DataLayer.Services
{
    public class RateHistoryRep : IRateHistoryRep
    {

        private NobatPlusContext _context;
        public RateHistoryRep(NobatPlusContext context)
        {
            _context = context;
        }

        public async Task<BitResultObject> AddRateHistoryAsync(RateHistory RateHistory)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.RateHistories.Add(RateHistory);
                await _context.SaveChangesAsync();
                result.ID = RateHistory.ID;
                _context.Entry(RateHistory).State = EntityState.Detached;

            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;

        }

        public async Task<BitResultObject> EditRateHistoryAsync(RateHistory RateHistory)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.RateHistories.Update(RateHistory);
                await _context.SaveChangesAsync();
                result.ID = RateHistory.ID;
                _context.Entry(RateHistory).State = EntityState.Detached;

            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;

        }

        public async Task<BitResultObject> ExistRateHistoryAsync(long RateHistoryId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                result.Status = await _context.RateHistories
                .AsNoTracking()
                .AnyAsync(x => x.ID == RateHistoryId);
                result.ID = RateHistoryId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;

        }

        public async Task<ListResultObject<RateHistoryDTO>> GetAllRateHistoriesAsync(
      long customerId = 0,
      long stylistId = 0,
       long bookingId = 0,
      long rateQuestionId = 0,
      int pageIndex = 1,
      int pageSize = 20,
      string searchText = "",
      string sortQuery = "")
        {
            ListResultObject<RateHistoryDTO> results = new ListResultObject<RateHistoryDTO>();
            try
            {
                IQueryable<RateHistory> query = _context.RateHistories
                    .Include(x => x.RateQuestion)
                    .Include(x => x.Stylist).ThenInclude(x => x.Person)
                    .Include(x => x.Customer).ThenInclude(x => x.Person)
                    .AsNoTracking();

                if (stylistId > 0)
                    query = query.Where(x => x.StylistID == stylistId);


                if (bookingId > 0)
                    query = query.Where(x => x.BookingID == bookingId);

                if (customerId > 0)
                    query = query.Where(x => x.CustomerID == customerId);

                if (rateQuestionId > 0)
                    query = query.Where(x => x.RateQuestionID == rateQuestionId);

                if (!string.IsNullOrEmpty(searchText))
                {
                    query = query.Where(x =>
                        (!string.IsNullOrEmpty(x.RateDate.ToString()) && x.RateDate.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.RateQuestion.RateQuestionText) && x.RateQuestion.RateQuestionText.Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.RateScore.ToString()) && x.RateScore.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText)) ||
                        (x.CreateDate.HasValue && x.CreateDate.Value.ToString().Contains(searchText)) ||
                        (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString().Contains(searchText))
                    );
                }

                results.TotalCount = await query.CountAsync();
                results.PageCount = DbTools.GetPageCount(results.TotalCount, pageSize);

                results.Results = await query
                    .OrderByDescending(x => x.CreateDate)
                    .SortBy(sortQuery)
                    .ToPaging(pageIndex, pageSize)
                    .Select(r => new RateHistoryDTO
                    {
                        ID = r.ID,
                        StylistID = r.StylistID,
                        CustomerID = r.CustomerID,
                        RateQuestionID = r.RateQuestionID,
                        RateScore = r.RateScore,
                        RateDate = r.RateDate,
                        Description = r.Description,
                        Stylist = r.Stylist,
                        Customer = r.Customer,
                        RateQuestion = r.RateQuestion,
                        UpdateDate = r.UpdateDate,
                        CreateDate = r.CreateDate,

                        // محاسباتی
                        AvgScorePerQuestion = _context.RateHistories
                            .Where(x => x.StylistID == r.StylistID && x.RateQuestionID == r.RateQuestionID)
                            .Average(x => x.RateScore),

                        AvgScoreForStylist = _context.RateHistories
                            .Where(x => x.StylistID == r.StylistID)
                            .Average(x => x.RateScore)
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                results.Status = false;
                results.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return results;
        }


        public async Task<RowResultObject<RateHistoryDTO>> GetRateHistoryByIdAsync(long rateHistoryId)
        {
            RowResultObject<RateHistoryDTO> result = new RowResultObject<RateHistoryDTO>();
            try
            {
                result.Result = await _context.RateHistories
                    .Include(x => x.RateQuestion)
                    .Include(x => x.Stylist).ThenInclude(x=> x.Person)
                    .Include(x => x.Customer).ThenInclude(x => x.Person)
                    .Where(x => x.ID == rateHistoryId)
                    .Select(r => new RateHistoryDTO
                    {
                        ID = r.ID,
                        StylistID = r.StylistID,
                        CustomerID = r.CustomerID,
                        RateQuestionID = r.RateQuestionID,
                        RateScore = r.RateScore,
                        RateDate = r.RateDate,
                        Description = r.Description,
                        Stylist = r.Stylist,
                        Customer = r.Customer,
                        RateQuestion = r.RateQuestion,
                        CreateDate = r.CreateDate,
                        UpdateDate = r.UpdateDate,

                        AvgScorePerQuestion = _context.RateHistories
                            .Where(x => x.StylistID == r.StylistID && x.RateQuestionID == r.RateQuestionID)
                            .Average(x => x.RateScore),

                        AvgScoreForStylist = _context.RateHistories
                            .Where(x => x.StylistID == r.StylistID)
                            .Average(x => x.RateScore)
                    })
                    .AsNoTracking()
                    .SingleOrDefaultAsync();

                if (result.Result == null)
                    throw new Exception("RateHistory not found");
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }


        public async Task<BitResultObject> RemoveRateHistoryAsync(RateHistory RateHistory)
        {
            BitResultObject result = new BitResultObject();
            try
            {

                _context.RateHistories.Remove(RateHistory);
                await _context.SaveChangesAsync();
                result.ID = RateHistory.ID;
                _context.Entry(RateHistory).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;

        }

        public async Task<BitResultObject> RemoveRateHistoryAsync(long RateHistoryId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var RateHistoryDTO = await GetRateHistoryByIdAsync(RateHistoryId);
                var RateHistory = new RateHistory()
                {
                    CreateDate = RateHistoryDTO.Result.CreateDate,
                    UpdateDate = RateHistoryDTO.Result.UpdateDate,
                    RateDate = RateHistoryDTO.Result.RateDate,
                    CustomerID = RateHistoryDTO.Result.CustomerID,
                    Description = RateHistoryDTO.Result.Description,
                    ID = RateHistoryDTO.Result.ID,
                    RateQuestionID = RateHistoryDTO.Result.RateQuestionID,
                    StylistID = RateHistoryDTO.Result.StylistID,
                    RateScore = RateHistoryDTO.Result.RateScore,
                };
                result = await RemoveRateHistoryAsync(RateHistory);
                await _context.SaveChangesAsync();
                _context.Entry(RateHistory).State = EntityState.Detached;
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
