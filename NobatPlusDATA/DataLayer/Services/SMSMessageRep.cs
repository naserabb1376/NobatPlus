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
    public class SMSMessageRep : ISMSMessageRep
    {

        private NobatPlusContext _context;
        public SMSMessageRep(NobatPlusContext context)
        {
            _context = context;
        }

        public async Task<BitResultObject> AddSMSMessageAsync(SMSMessage SMSMessage)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                await _context.SMSMessages.AddAsync(SMSMessage);
                await _context.SaveChangesAsync();
                result.ID = SMSMessage.ID;
                _context.Entry(SMSMessage).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
           
        }

        public async Task<BitResultObject> EditSMSMessageAsync(SMSMessage SMSMessage)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.SMSMessages.Update(SMSMessage);
                await _context.SaveChangesAsync();
                result.ID = SMSMessage.ID;
                _context.Entry(SMSMessage).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
           
        }

        public async Task<BitResultObject> ExistSMSMessageAsync(long SMSMessageId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                result.Status = await _context.SMSMessages
                .AsNoTracking()
                .AnyAsync(x => x.ID == SMSMessageId);
                result.ID = SMSMessageId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<ListResultObject<SMSMessage>> GetAllSMSMessagesAsync(long personId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="")
        {
            ListResultObject<SMSMessage> results = new ListResultObject<SMSMessage>();
            try
            {
                IQueryable<SMSMessage> query;

                if (personId == 0)
                {
                    query = _context.SMSMessages
                        .AsNoTracking().Include(x => x.Person)
                        .Where(x =>
                            (!string.IsNullOrEmpty(x.Message) && x.Message.Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.SentDate.ToString()) && x.SentDate.ToString().Contains(searchText)) ||
                            (x.CreateDate.HasValue && x.CreateDate.Value.ToString().Contains(searchText)) ||
                            (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString().Contains(searchText))
                        );
                }
                else
                {
                    query = _context.SMSMessages.Include(x => x.Person)
                        .AsNoTracking()
                        .Where(x =>
                            (
                                (!string.IsNullOrEmpty(x.Message) && x.Message.Contains(searchText)) ||
                                (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText)) ||
                                (!string.IsNullOrEmpty(x.SentDate.ToString()) && x.SentDate.ToString().Contains(searchText)) ||
                                (x.CreateDate.HasValue && x.CreateDate.Value.ToString().Contains(searchText)) ||
                                (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString().Contains(searchText))
                            )
                        );
                }
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

        public async Task<RowResultObject<SMSMessage>> GetSMSMessageByIdAsync(long SMSMessageId)
        {
            RowResultObject<SMSMessage> result = new RowResultObject<SMSMessage>();
            try
            {
                result.Result = await _context.SMSMessages
                .AsNoTracking().Include(x=> x.Person)
                .SingleOrDefaultAsync(x => x.ID == SMSMessageId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<BitResultObject> RemoveSMSMessageAsync(SMSMessage SMSMessage)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.SMSMessages.Remove(SMSMessage);
                await _context.SaveChangesAsync();
                result.ID = SMSMessage.ID;
                _context.Entry(SMSMessage).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
           
        }

        public async Task<BitResultObject> RemoveSMSMessageAsync(long SMSMessageId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var SMSMessage = await GetSMSMessageByIdAsync(SMSMessageId);
                result = await RemoveSMSMessageAsync(SMSMessage.Result);
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
