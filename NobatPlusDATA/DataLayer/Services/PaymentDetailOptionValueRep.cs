using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;

namespace NobatPlusDATA.DataLayer.Services
{
    public class PaymentDetailOptionValueRep : IPaymentDetailOptionValueRep
    {
        private readonly NobatPlusContext _context;

        public PaymentDetailOptionValueRep(NobatPlusContext context)
        {
            _context = context;
        }

        public async Task<BitResultObject> AddPaymentDetailOptionValuesAsync(List<PaymentDetailOptionValue> optionValues)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                await _context.PaymentDetailOptionValues.AddRangeAsync(optionValues);
                await _context.SaveChangesAsync();
                result.ID = optionValues.FirstOrDefault()?.PaymentDetailID ?? 0;
                foreach (var item in optionValues) _context.Entry(item).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> EditPaymentDetailOptionValuesAsync(List<PaymentDetailOptionValue> optionValues)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.PaymentDetailOptionValues.UpdateRange(optionValues);
                await _context.SaveChangesAsync();
                result.ID = optionValues.FirstOrDefault()?.PaymentDetailID ?? 0;
                foreach (var item in optionValues) _context.Entry(item).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> RemovePaymentDetailOptionValuesAsync(List<PaymentDetailOptionValue> optionValues)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.PaymentDetailOptionValues.RemoveRange(optionValues);
                await _context.SaveChangesAsync();
                result.ID = optionValues.FirstOrDefault()?.PaymentDetailID ?? 0;
                foreach (var item in optionValues) _context.Entry(item).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> RemovePaymentDetailOptionValuesAsync(List<(long PaymentDetailId, long ServiceOptionValueId)> ids)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var rows = new List<PaymentDetailOptionValue>();
                foreach (var id in ids)
                {
                    var row = await GetPaymentDetailOptionValueByIdAsync(id.PaymentDetailId, id.ServiceOptionValueId);
                    if (row.Result != null) rows.Add(row.Result);
                }

                if (!rows.Any())
                {
                    result.Status = false;
                    result.ErrorMessage = "No matching payment detail option values found to remove.";
                    return result;
                }

                result = await RemovePaymentDetailOptionValuesAsync(rows);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> ExistPaymentDetailOptionValueAsync(long paymentDetailId, long serviceOptionValueId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                result.Status = await _context.PaymentDetailOptionValues.AsNoTracking()
                    .AnyAsync(x => x.PaymentDetailID == paymentDetailId && x.ServiceOptionValueID == serviceOptionValueId);
                result.ID = paymentDetailId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<ListResultObject<PaymentDetailOptionValue>> GetAllPaymentDetailOptionValuesAsync(long paymentDetailId = 0, long serviceOptionValueId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "", string sortQuery = "")
        {
            ListResultObject<PaymentDetailOptionValue> results = new ListResultObject<PaymentDetailOptionValue>();
            try
            {
                var query = _context.PaymentDetailOptionValues
                    .Include(x => x.PaymentDetail).ThenInclude(x => x.ServiceManagement)
                    .Include(x => x.ServiceOptionValue).ThenInclude(x => x.ServiceOption)
                    .AsNoTracking();

                if (paymentDetailId > 0) query = query.Where(x => x.PaymentDetailID == paymentDetailId);
                if (serviceOptionValueId > 0) query = query.Where(x => x.ServiceOptionValueID == serviceOptionValueId);

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    query = query.Where(x =>
                        x.PaymentDetail.ServiceManagement.ServiceName.Contains(searchText) ||
                        x.PaymentDetail.AppliedOptionSummary.Contains(searchText) ||
                        x.ServiceOptionValue.ValueName.Contains(searchText) ||
                        x.ServiceOptionValue.ServiceOption.OptionName.Contains(searchText));
                }

                results.TotalCount = query.Count();
                results.PageCount = DbTools.GetPageCount(results.TotalCount, pageSize);
                results.Results = await query.OrderByDescending(x => x.PaymentDetailID).SortBy(sortQuery).ToPaging(pageIndex, pageSize).ToListAsync();
            }
            catch (Exception ex)
            {
                results.Status = false;
                results.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return results;
        }

        public async Task<RowResultObject<PaymentDetailOptionValue>> GetPaymentDetailOptionValueByIdAsync(long paymentDetailId, long serviceOptionValueId)
        {
            RowResultObject<PaymentDetailOptionValue> result = new RowResultObject<PaymentDetailOptionValue>();
            try
            {
                result.Result = await _context.PaymentDetailOptionValues
                    .Include(x => x.PaymentDetail).ThenInclude(x => x.ServiceManagement)
                    .Include(x => x.ServiceOptionValue).ThenInclude(x => x.ServiceOption)
                    .AsNoTracking()
                    .SingleOrDefaultAsync(x => x.PaymentDetailID == paymentDetailId && x.ServiceOptionValueID == serviceOptionValueId);
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
