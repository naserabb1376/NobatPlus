using Domain;
using Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
    public class StylistRep : IStylistRep
    {

        private NobatPlusContext _context;
        public StylistRep(NobatPlusContext context)
        {
            _context = context;
        }

        public async Task<BitResultObject> AddStylistAsync(Stylist Stylist)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                await _context.Stylists.AddAsync(Stylist);
                await _context.SaveChangesAsync();
                result.ID = Stylist.ID;
                _context.Entry(Stylist).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<BitResultObject> EditStylistAsync(Stylist Stylist)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.Stylists.Update(Stylist);
                await _context.SaveChangesAsync();
                result.ID = Stylist.ID;
                _context.Entry(Stylist).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<BitResultObject> ExistStylistAsync(string fieldValue, string fieldName)
        {
            BitResultObject result = new BitResultObject();
            long stylistId = 0;
            try
            {
                switch (fieldName.ToLower().Trim())
                {
                    case "personid":
                        {
                            var theStylist = await _context.Stylists.AsNoTracking().FirstOrDefaultAsync(x => x.PersonID == long.Parse(fieldValue)) ?? new Stylist();
                            stylistId = theStylist.ID;
                            break;
                        }
                    case "stylistid":
                    default:
                        {
                            var theStylist = await _context.Stylists.AsNoTracking().FirstOrDefaultAsync(x => x.ID == long.Parse(fieldValue)) ?? new Stylist();
                            stylistId = theStylist.PersonID;
                            break;
                        }
                }
                result.ID = stylistId;
                result.Status = stylistId > 0;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<ListResultObject<StylistDTO>> GetAllStylistsAsync(
    long parentId = 0,
    long serviceManagementId = 0,
    long jobTypeId = 0,
    long discountId = 0,
    long cityId = 0,
    int pageIndex = 1,
    int pageSize = 20,
    string searchText = "",
    string sortQuery = "",
    FindLocationRequestBody findLocation = null)
        {
            ListResultObject<StylistDTO> results = new ListResultObject<StylistDTO>();

            try
            {
                IQueryable<Stylist> query;

                // 🟡 منبع داده اصلی بر اساس پارامترهای ورودی
                if (serviceManagementId > 0)
                {
                    query = _context.StylistServices
                        .Where(s => s.ServiceManagementID == serviceManagementId)
                        .Select(s => s.Stylist);
                }
                else if (jobTypeId > 0)
                {
                    query = _context.Stylists.Where(x => x.JobTypeID == jobTypeId);
                }
                else if (discountId > 0)
                {
                    var fromDiscountAssignments = _context.DiscountAssignments
                        .Where(d => d.DiscountId == discountId)
                        .Select(d => d.Stylist);

                    var fromServiceDiscounts = _context.ServiceDiscounts
                        .Where(d => d.DiscountId == discountId)
                        .Select(d => d.Stylist);

                    var fromCustomerDiscounts = _context.CustomerDiscounts
                        .Where(d => d.DiscountId == discountId)
                        .Select(d => d.Stylist);

                    query = fromDiscountAssignments.Union(fromServiceDiscounts).Union(fromCustomerDiscounts);
                }
                else
                {
                    query = _context.Stylists.AsQueryable();

                    if (parentId > 0)
                    {
                        if (parentId >= 0)
                            query = query.Where(x => x.StylistParentID == parentId);
                        else
                            query = query.Where(x => x.StylistParentID > 0 || !x.IsWorkShop);
                    }
                }

                // 🧭 Include های مشترک
                query = query
                    .Include(x => x.Person).ThenInclude(x => x.Address).ThenInclude(x => x.City)
                    .Include(x => x.JobType)
                    .Include(x => x.StylistServices).ThenInclude(x => x.ServiceManagement)
                    .AsNoTracking();

                // 📍 فیلتر موقعیت مکانی
                if (findLocation != null)
                {
                    double personLat = 0, personLng = 0;
                    query = query.Where(p =>
                        p.Person.Address != null &&
                        double.TryParse(p.Person.Address.AddressLocationVerticalPoint, out personLat) &&
                        double.TryParse(p.Person.Address.AddressLocationHorizentalPoint, out personLng) &&
                        GeoHelper.CalculateDistance(findLocation.LocationLatitude, findLocation.LocationLongitude, personLat, personLng) <= findLocation.RadiusKm);
                }

                // 🔍 فیلتر سرچ (مشترک)
                if (!string.IsNullOrEmpty(searchText))
                {
                    query = query.Where(x =>
                        (x.Person.FirstName != null && x.Person.FirstName.Contains(searchText)) ||
                        (x.Person.LastName != null && x.Person.LastName.Contains(searchText)) ||
                        (x.Person.NaCode != null && x.Person.NaCode.Contains(searchText)) ||
                        (x.Person.PhoneNumber != null && x.Person.PhoneNumber.Contains(searchText)) ||
                        (x.Person.Email != null && x.Person.Email.Contains(searchText)) ||
                        (x.Person.Description != null && x.Person.Description.Contains(searchText)) ||
                        (x.Person.Address.City.CityName != null && x.Person.Address.City.CityName.Contains(searchText)) ||
                        (x.JobType.JobTitle != null && x.JobType.JobTitle.Contains(searchText)) ||
                        (x.Specialty != null && x.Specialty.Contains(searchText)) ||
                        (x.StylistServices.Any(s => s.ServiceManagement.ServiceName.Contains(searchText))) ||
                        (x.Description != null && x.Description.Contains(searchText))
                    );
                }

                // 📊 شمارش و صفحه‌بندی
                results.TotalCount = await query.CountAsync();
                results.PageCount = DbTools.GetPageCount(results.TotalCount, pageSize);

                // 🧾 خروجی با DTO
                results.Results = await query
                    .OrderByDescending(x => x.CreateDate)
                    .SortBy(sortQuery)
                    .ToPaging(pageIndex, pageSize)
                    .Select(r => new StylistDTO
                    {
                        ID = r.ID,
                        Description = r.Description ?? "",
                        UpdateDate = r.UpdateDate,
                        CreateDate = r.CreateDate,
                        AccountStatus = r.AccountStatus ?? "",
                        PayMethod = r.PayMethod ?? "",
                        IsWorkShop = r.IsWorkShop,
                        GenderAccepted = r.GenderAccepted ?? "",
                        JobType = r.JobType,
                        JobTypeID = r.JobTypeID,
                        Person = r.Person,
                        PersonID = r.PersonID,
                        Specialty = r.Specialty ?? "",
                        StylistBio = r.StylistBio ?? "",
                        StylistName = r.StylistName ?? "",
                        StylistParentID = r.StylistParentID,
                        WorkShopDepositAmount = r.WorkShopDepositAmount,
                        WorkShopInteractMode = r.WorkShopInteractMode ?? "",
                        WorkShopRentAmount = r.WorkShopRentAmount,
                        YearsOfExperience = r.YearsOfExperience,
                        StylistImagePath = _context.Images.Any(x=> x.EntityType.ToLower() == "stylist" && x.ForeignKeyId == r.ID) ? $"https://nobatplusapi.mtcoding.ir/FileCenter/downloadfile?fileType=images&rowId=0&foreignkeyId={r.ID}&entityName=stylist" : "",
                        StylistServices = r.StylistServices
    .Select(s => new StylistService
    {
        StylistID = s.StylistID,
        ServiceManagementID = s.ServiceManagementID,
        ServicePrice = s.ServicePrice,
        ServiceDuration = s.ServiceDuration,
        DepositPercent = s.DepositPercent,
        ServiceManagement = s.ServiceManagement
    })
    .ToList(),

                        // محاسبه امتیاز آرایشگر
                        AvgScoreForStylist = _context.RateHistories
                            .Where(x => x.StylistID == r.ID)
                            .Any()
                                ? _context.RateHistories.Where(x => x.StylistID == r.ID).Average(r => r.RateScore)
                                : 0,

                                 TodayBookingsCount = _context.Bookings
                    .Count(b => b.StylistID == r.ID && b.BookingDate.Date == DateTime.Today),

                        TotalBookingsCount = _context.Bookings
                    .Count(b => b.StylistID == r.ID),

                        TotalCustomersCount = _context.Bookings
                    .Where(b => b.StylistID == r.ID)
                    .Select(b => b.CustomerID)
                    .Distinct()
                    .Count(),
                    IsOnLeaveNow = _context.StylistPacifics
    .Any(p => p.StylistID == r.ID &&
              DateTime.Now >= p.PacificStartDate &&
              DateTime.Now <= p.PacificEndDate)
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


        public async Task<RowResultObject<StylistDTO>> GetStylistByIdAsync(long StylistId)
        {
            RowResultObject<StylistDTO> result = new RowResultObject<StylistDTO>();
            try
            {
                result.Result = await _context.Stylists.Include(x => x.Person).ThenInclude(x => x.Address).ThenInclude(x => x.City)
               .Include(x => x.JobType).Include(x => x.StylistServices).ThenInclude(x => x.ServiceManagement)
                    .Where(x=> x.ID == StylistId)
                     .Select(r => new StylistDTO
                     {
                         ID = r.ID,
                         Description = r.Description ?? "",
                         UpdateDate = r.UpdateDate,
                         CreateDate = r.CreateDate,
                         AccountStatus = r.AccountStatus ?? "",
                         PayMethod = r.PayMethod ?? "",
                         IsWorkShop = r.IsWorkShop,
                         GenderAccepted = r.GenderAccepted ?? "",
                         JobType = r.JobType,
                         JobTypeID = r.JobTypeID,
                         Person = r.Person,
                         PersonID = r.PersonID,
                         Specialty = r.Specialty ?? "",
                         StylistBio = r.StylistBio ?? "",
                         StylistName = r.StylistName ?? "",
                         StylistParentID = r.StylistParentID,
                         WorkShopDepositAmount = r.WorkShopDepositAmount,
                         WorkShopInteractMode = r.WorkShopInteractMode ?? "",
                         WorkShopRentAmount = r.WorkShopRentAmount,
                         YearsOfExperience = r.YearsOfExperience,
                         StylistImagePath = _context.Images.Any(x => x.EntityType.ToLower() == "stylist" && x.ForeignKeyId == r.ID) ? $"https://nobatplusapi.mtcoding.ir/FileCenter/downloadfile?fileType=images&rowId=0&foreignkeyId={r.ID}&entityName=stylist" : "",
                         StylistServices = r.StylistServices
    .Select(s => new StylistService
    {
        StylistID = s.StylistID,
        ServiceManagementID = s.ServiceManagementID,
        ServicePrice = s.ServicePrice,
        ServiceDuration = s.ServiceDuration,
        DepositPercent = s.DepositPercent,
        ServiceManagement = s.ServiceManagement
    })
    .ToList(),
                         // محاسباتی
                         AvgScoreForStylist = _context.RateHistories
                         .Where(x => x.StylistID == r.ID)
                         .Any()
                             ? _context.RateHistories.Where(x => x.StylistID == r.ID).Average(r => r.RateScore)
                             : 0,
                         TodayBookingsCount = _context.Bookings
                    .Count(b => b.StylistID == r.ID && b.BookingDate.Date == DateTime.Today),

                         TotalBookingsCount = _context.Bookings
                    .Count(b => b.StylistID == r.ID),

                         TotalCustomersCount = _context.Bookings
                    .Where(b => b.StylistID == r.ID)
                    .Select(b => b.CustomerID)
                    .Distinct()
                    .Count(),
                         IsOnLeaveNow = _context.StylistPacifics
    .Any(p => p.StylistID == r.ID &&
              DateTime.Now >= p.PacificStartDate &&
              DateTime.Now <= p.PacificEndDate)
                     })
                    .AsNoTracking().SingleOrDefaultAsync();

         
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;

        }

        public async Task<BitResultObject> RemoveStylistAsync(Stylist Stylist)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.Stylists.Remove(Stylist);
                await _context.SaveChangesAsync();
                result.ID = Stylist.ID;
                _context.Entry(Stylist).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
           
        }

        public async Task<BitResultObject> RemoveStylistAsync(long StylistId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var stylistDTO = await GetStylistByIdAsync(StylistId);
                var stylist = new Stylist()
                 {
                     ID = stylistDTO.Result.ID,
                     Description = stylistDTO.Result.Description,
                     UpdateDate = stylistDTO.Result.UpdateDate,
                     CreateDate = stylistDTO.Result.CreateDate,
                     AccountStatus = stylistDTO.Result.AccountStatus,
                     PayMethod = stylistDTO.Result.PayMethod,
                     Bookings = stylistDTO.Result.Bookings,
                     CustomerDiscounts = stylistDTO.Result.CustomerDiscounts,
                     IsWorkShop = stylistDTO.Result.IsWorkShop,
                     GenderAccepted = stylistDTO.Result.GenderAccepted,
                     DiscountAssignments = stylistDTO.Result.DiscountAssignments,
                     JobType = stylistDTO.Result.JobType,
                     JobTypeID = stylistDTO.Result.JobTypeID,
                     Person = stylistDTO.Result.Person,
                     PersonID = stylistDTO.Result.PersonID,
                     RateHistories = stylistDTO.Result.RateHistories,
                     ServiceDiscounts = stylistDTO.Result.ServiceDiscounts,
                     SocialNetworks = stylistDTO.Result.SocialNetworks,
                     Specialty = stylistDTO.Result.Specialty,
                     StylistBio = stylistDTO.Result.StylistBio,
                     StylistName = stylistDTO.Result.StylistName,
                     StylistParentID = stylistDTO.Result.StylistParentID,
                     StylistServices = stylistDTO.Result.StylistServices,
                     WorkShopDepositAmount = stylistDTO.Result.WorkShopDepositAmount,
                     WorkShopInteractMode = stylistDTO.Result.WorkShopInteractMode,
                     WorkShopRentAmount = stylistDTO.Result.WorkShopRentAmount,
                     WorkTimes = stylistDTO.Result.WorkTimes,
                     YearsOfExperience = stylistDTO.Result.YearsOfExperience,
                 };
                result = await RemoveStylistAsync(stylist);
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
