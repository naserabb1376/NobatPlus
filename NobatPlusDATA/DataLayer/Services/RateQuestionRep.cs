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

namespace NobatPlusDATA.DataLayer.Services
{
    public class RateQuestionRep : IRateQuestionRep
    {

        private NobatPlusContext _context;
        public RateQuestionRep(NobatPlusContext context)
        {
            _context = context;
        }

        public async Task<BitResultObject> AddRateQuestionAsync(RateQuestion RateQuestion)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                await _context.RateQuestions.AddAsync(RateQuestion);
                await _context.SaveChangesAsync();
                result.ID = RateQuestion.ID;
                _context.Entry(RateQuestion).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;

        }

        public async Task<BitResultObject> EditRateQuestionAsync(RateQuestion RateQuestion)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.RateQuestions.Update(RateQuestion);
                await _context.SaveChangesAsync();
                result.ID = RateQuestion.ID;
                _context.Entry(RateQuestion).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;

        }

        public async Task<BitResultObject> ExistRateQuestionAsync(long RateQuestionId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                result.Status = await _context.RateQuestions
                .AsNoTracking()
                .AnyAsync(x => x.ID == RateQuestionId);
                result.ID = RateQuestionId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;

        }

        public async Task<ListResultObject<RateQuestion>> GetAllRateQuestionsAsync(int pageIndex = 1, int pageSize = 20, string searchText = "", string sortQuery = "")
        {
            ListResultObject<RateQuestion> results = new ListResultObject<RateQuestion>();
            try
            {
                IQueryable<RateQuestion> query;
                query = _context.RateQuestions
                    .AsNoTracking()
                    .Where(x =>
                        (
                            (!string.IsNullOrEmpty(x.RateQuestionText) && x.RateQuestionText.Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText)) ||
                            (x.CreateDate.HasValue && x.CreateDate.Value.ToString().Contains(searchText)) ||
                            (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString().Contains(searchText))
                        )
                    );
                results.TotalCount = query.Count();
                results.PageCount = DbTools.GetPageCount(results.TotalCount, pageSize);
                results.Results = await query.OrderByDescending(x => x.CreateDate)
                .SortBy(sortQuery).ToPaging(pageIndex, pageSize)
                .ToListAsync();
            }
            catch (Exception ex)
            {
                results.Status = false;
                results.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return results;

        }

        public async Task<RowResultObject<RateQuestion>> GetRateQuestionByIdAsync(long RateQuestionId)
        {
            RowResultObject<RateQuestion> result = new RowResultObject<RateQuestion>();
            try
            {
                result.Result = await _context.RateQuestions
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.ID == RateQuestionId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;

        }

        public async Task<BitResultObject> RemoveRateQuestionAsync(RateQuestion RateQuestion)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.RateQuestions.Remove(RateQuestion);
                await _context.SaveChangesAsync();
                result.ID = RateQuestion.ID;
                _context.Entry(RateQuestion).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;

        }

        public async Task<BitResultObject> RemoveRateQuestionAsync(long RateQuestionId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var RateQuestion = await GetRateQuestionByIdAsync(RateQuestionId);
                result = await RemoveRateQuestionAsync(RateQuestion.Result);
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
