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
    public class PaymentDetailRep : IPaymentDetailRep
    {

        private NobatPlusContext _context;
        public PaymentDetailRep(NobatPlusContext context)
        {
            _context = context;
        }

        public async Task<BitResultObject> AddPaymentDetailAsync(PaymentDetail PaymentDetail)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                await _context.PaymentDetails.AddAsync(PaymentDetail);
                await _context.SaveChangesAsync();
                result.ID = PaymentDetail.ID;
                _context.Entry(PaymentDetail).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
           
        }

        public async Task<BitResultObject> EditPaymentDetailAsync(PaymentDetail PaymentDetail)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.PaymentDetails.Update(PaymentDetail);
                await _context.SaveChangesAsync();
                result.ID = PaymentDetail.ID;
                _context.Entry(PaymentDetail).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<BitResultObject> ExistPaymentDetailAsync(long PaymentDetailId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                result.Status = await _context.PaymentDetails
                .AsNoTracking()
                .AnyAsync(x => x.ID == PaymentDetailId);
                result.ID = PaymentDetailId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<ListResultObject<PaymentDetail>> GetAllPaymentDetailsAsync(long stylistId = 0, long ServiceId = 0, long paymentId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="")
        {
            ListResultObject<PaymentDetail> results = new ListResultObject<PaymentDetail>();
            try
            {
                IQueryable<PaymentDetail> query = _context.PaymentDetails.Include(x=> x.Payment).Include(x => x.ServiceManagement).Include(x => x.Stylist).ThenInclude(x=> x.Person)
                         .AsNoTracking();

                if (stylistId > 0)
                {
                    query = query.Where(x => x.StylistID == stylistId);
                }

                if (ServiceId > 0)
                {
                    query = query.Where(x => x.ServiceManagementID == ServiceId);
                }

                if (paymentId > 0)
                {
                    query = query.Where(x => x.PaymentID == paymentId);
                }



                query = query
                       .Where(x =>
                        
                           
                               (!string.IsNullOrEmpty(x.Payment.PaymentStatus.ToString()) && x.Payment.PaymentStatus.ToString().Contains(searchText)) ||
                               (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText)) ||
                               (!string.IsNullOrEmpty(x.ServiceManagement.ServiceName) && x.ServiceManagement.ServiceName.Contains(searchText)) ||
                               (!string.IsNullOrEmpty(x.Stylist.Person.FirstName) && x.Stylist.Person.FirstName.Contains(searchText)) ||
                               (!string.IsNullOrEmpty(x.Stylist.Person.LastName) && x.Stylist.Person.LastName.Contains(searchText)) ||
                               (!string.IsNullOrEmpty(x.Stylist.StylistName) && x.Stylist.StylistName.Contains(searchText)) ||
                               (!string.IsNullOrEmpty(x.Payment.DepositAmount.ToString()) && x.Payment.DepositAmount.ToString().Contains(searchText)) ||
                               (!string.IsNullOrEmpty(x.Payment.AllPaymentAmount.ToString()) && x.Payment.AllPaymentAmount.ToString().Contains(searchText)) ||
                               (!string.IsNullOrEmpty(x.Payment.TotalServiceAmount.ToString()) && x.Payment.TotalServiceAmount.ToString().Contains(searchText)) ||
                               (!string.IsNullOrEmpty(x.Payment.PlarformAmount.ToString()) && x.Payment.PlarformAmount.ToString().Contains(searchText)) ||
                               (!string.IsNullOrEmpty(x.Payment.StylistAmount.ToString()) && x.Payment.StylistAmount.ToString().Contains(searchText)) ||
                               (!string.IsNullOrEmpty(x.DiscountAmount.ToString()) && x.DiscountAmount.ToString().Contains(searchText)) ||
                               (!string.IsNullOrEmpty(x.DiscountPercent.ToString()) && x.DiscountPercent.ToString().Contains(searchText)) ||
                               (!string.IsNullOrEmpty(x.StylistServiceAmount.ToString()) && x.StylistServiceAmount.ToString().Contains(searchText)) ||
                               (x.CreateDate.HasValue && x.CreateDate.Value.ToString().Contains(searchText)) ||
                               (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString().Contains(searchText))
                           
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

        public async Task<RowResultObject<PaymentDetail>> GetPaymentDetailByIdAsync(long PaymentDetailId)
        {
            RowResultObject<PaymentDetail> result = new RowResultObject<PaymentDetail>();
            try
            {
                result.Result = await _context.PaymentDetails.Include(x=> x.Payment).Include(x => x.ServiceManagement).Include(x => x.Stylist).ThenInclude(x=> x.Person)
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.ID == PaymentDetailId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<BitResultObject> RemovePaymentDetailAsync(PaymentDetail PaymentDetail)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.PaymentDetails.Remove(PaymentDetail);
                await _context.SaveChangesAsync();
                result.ID = PaymentDetail.ID;
                _context.Entry(PaymentDetail).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<BitResultObject> RemovePaymentDetailAsync(long PaymentDetailId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var PaymentDetail = await GetPaymentDetailByIdAsync(PaymentDetailId);
                result = await RemovePaymentDetailAsync(PaymentDetail.Result);
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
