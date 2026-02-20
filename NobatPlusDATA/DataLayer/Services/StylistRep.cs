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
                            stylistId = theStylist.ID;
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
            List<long> serviceIds = null,
            long jobTypeId = 0,
            long discountId = 0,
            decimal fromPrice = 0,
            decimal toPrice = 0,
            long cityId = 0,
            int gender = 0,
            int pageIndex = 1,
            int pageSize = 20,
            string searchText = "",
            string sortQuery = "",
            FindLocationRequestBody findLocation = null)
        {
            if (serviceIds == null)
            {
                serviceIds = new List<long>();
            }
            var results = new ListResultObject<StylistDTO>();

            try
            {
                // ✅ همیشه از Entity Root شروع کن تا Include معتبر بماند
                IQueryable<Stylist> query = _context.Stylists.AsQueryable();

                if (parentId > 0)
                {
                    if (parentId >= 0)
                        query = query.Where(x => x.StylistParentID == parentId);
                    else
                        query = query.Where(x => x.StylistParentID > 0 || !x.IsWorkShop);
                }

                // 🟡 فیلترهای اصلی بر اساس پارامترها
                if (serviceIds.Count > 0)
                {
                    query = query.Where(st =>
                        st.StylistServices.Any(ss => serviceIds.Contains( ss.ServiceManagementID)));
                }
                if (jobTypeId > 0)
                {
                    query = query.Where(x => x.JobTypeID == jobTypeId);
                }
                if (discountId > 0)
                {
                    // ✅ به جای Select(d => d.Stylist) از ID ها استفاده کن (entity-root را حفظ می‌کند)
                    var stylistIds =
       _context.DiscountAssignments
           .Where(d => d.DiscountId == discountId && d.StylistId != null)
           .Select(d => d.StylistId!.Value)
       .Union(
       _context.ServiceDiscounts
           .Where(d => d.DiscountId == discountId && d.StylistId != null)
           .Select(d => d.StylistId!.Value))
       .Union(
       _context.CustomerDiscounts
           .Where(d => d.DiscountId == discountId && d.StylistId != null)
           .Select(d => d.StylistId))
       .Distinct();


                    query = query.Where(st => stylistIds.Contains(st.ID));
                }
            

                // ✅ اگر cityId داری (تو امضای متد هست ولی قبلاً استفاده نشده بود)
                if (cityId > 0)
                {
                    query = query.Where(x =>
                        x.Person != null &&
                        x.Person.Address != null &&
                        x.Person.Address.CityID == cityId);
                }

                if (gender > 0)
                {
                    query = query.Where(x => x.Person.Gender == gender);
                }

                if (fromPrice > 0)
                {
                    query = query.Where(st =>
                        st.StylistServices.Any(ss => ss.ServicePrice >= fromPrice));
                }

                if (toPrice > 0)
                {
                    query = query.Where(st =>
                        st.StylistServices.Any(ss => ss.ServicePrice <= toPrice));
                }

                // 🧭 Include های مشترک (الان معتبر است)
                query = query
                    .Include(x => x.Person).ThenInclude(x => x.Address).ThenInclude(x => x.City)
                    .Include(x => x.JobType)
                    .Include(x => x.StylistServices).ThenInclude(x => x.ServiceManagement)
                    .Include(x => x.WorkTimes)
                    .Include(x => x.SocialNetworks)
                    .AsNoTracking();

                // 📍 فیلتر موقعیت مکانی
                if (findLocation != null)
                {
                    double personLat = 0, personLng = 0;

                    query = query.Where(p =>
                        p.Person.Address != null &&
                        double.TryParse(p.Person.Address.AddressLocationVerticalPoint, out personLat) &&
                        double.TryParse(p.Person.Address.AddressLocationHorizentalPoint, out personLng) &&
                        GeoHelper.CalculateDistance(
                            findLocation.LocationLatitude,
                            findLocation.LocationLongitude,
                            personLat,
                            personLng
                        ) <= findLocation.RadiusKm
                    );
                }

                // 🔍 فیلتر سرچ
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    searchText = searchText.Trim();

                    var tokens = searchText
                        .Replace("-", " ")
                        .Replace(",", " ")
                        .Replace("،", " ")
                        .Split(" ", StringSplitOptions.RemoveEmptyEntries)
                        .ToList();

                    query = query.Where(x =>
                        (x.Person.FirstName != null && x.Person.FirstName.Contains(searchText)) ||
                        (x.Person.LastName != null && x.Person.LastName.Contains(searchText)) ||

                        // جستجوی ترکیبی نام و نام خانوادگی
                        (x.Person.FirstName != null && x.Person.LastName != null &&
                         (
                             (x.Person.FirstName + " " + x.Person.LastName).Contains(searchText) ||
                             (x.Person.FirstName + x.Person.LastName).Contains(searchText)
                         )) ||

                        (x.Person.NaCode != null && x.Person.NaCode.Contains(searchText)) ||
                        (x.Person.PhoneNumber != null && x.Person.PhoneNumber.Contains(searchText)) ||
                        (x.Person.Email != null && x.Person.Email.Contains(searchText)) ||
                        (x.Person.Description != null && x.Person.Description.Contains(searchText)) ||
                        (x.Person.Address.City.CityName != null && x.Person.Address.City.CityName.Contains(searchText)) ||
                        (x.JobType.JobTitle != null && x.JobType.JobTitle.Contains(searchText)) ||
                        (x.Specialty != null && x.Specialty.Contains(searchText)) ||

                        (
                            tokens.Count == 1
                                ? x.StylistServices.Any(s => s.ServiceManagement.ServiceName.Contains(tokens[0]))
                                : tokens.All(token =>
                                    x.StylistServices.Any(s => s.ServiceManagement.ServiceName.Contains(token))
                                  )
                        ) ||

                        (x.Description != null && x.Description.Contains(searchText))
                    );
                }

                // ✅ شمارش و صفحه‌بندی (حالا CountAsync خطا نمی‌دهد)
                results.TotalCount = await query.CountAsync();
                results.PageCount = DbTools.GetPageCount(results.TotalCount, pageSize);

                // 🧾 خروجی DTO
                results.Results = await query
                    .OrderByDescending(x => x.CreateDate)
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
                        RestTime = r.RestTime, 

                        StylistImagePath =
                            _context.Images.Any(x => x.EntityType.ToLower() == "stylist" && x.ForeignKeyId == r.ID)
                                ? $"https://nobatplusapi.mtcoding.ir/FileCenter/downloadfile?fileType=images&rowId=0&foreignkeyId={r.ID}&entityName=stylist"
                                : "",

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

                        SocialNetworks = r.SocialNetworks
                            .Select(s => new SocialNetworkDTO
                            {
                                AccountLink = s.AccountLink,
                                PhoneNumber = s.PhoneNumber,
                                SocialNetworkIcon = s.SocialNetworkIcon,
                                SocialNetworkName = s.SocialNetworkName
                            })
                            .ToList(),

                        WorkTimes = r.WorkTimes
                            .Select(s => new WorkTimeDTO
                            {
                                DayOfWeek = s.DayOfWeek,
                                WorkStartTime = s.WorkStartTime,
                                WorkEndTime = s.WorkEndTime
                            })
                            .ToList(),

                        AvgScoreForStylist = _context.RateHistories
                            .Where(x => x.StylistID == r.ID)
                            .Any()
                                ? _context.RateHistories.Where(x => x.StylistID == r.ID).Average(rr => rr.RateScore)
                                : 0,

                        RecommendPercent = _context.RateHistories
                            .Where(x => x.StylistID == r.ID && x.RateQuestionID == 5)
                            .Any()
                                ? (
                                    _context.RateHistories.Count(x => x.StylistID == r.ID && x.RateQuestionID == 5 && x.RateScore == 5.0) * 100.0
                                    /
                                    _context.RateHistories.Count(x => x.StylistID == r.ID && x.RateQuestionID == 5)
                                  )
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
                    .SortBy(sortQuery)
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
               .Include(x => x.WorkTimes).Include(x => x.SocialNetworks)
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
                         RestTime =r.RestTime,

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
                         SocialNetworks = r.SocialNetworks
    .Select(s => new SocialNetworkDTO
    {
        AccountLink = s.AccountLink,
        PhoneNumber = s.PhoneNumber,
        SocialNetworkIcon = s.SocialNetworkIcon,
        SocialNetworkName = s.SocialNetworkName
    })
    .ToList(),
                         WorkTimes = r.WorkTimes
    .Select(s => new WorkTimeDTO
    {
        DayOfWeek = s.DayOfWeek,
        WorkStartTime = s.WorkStartTime,
        WorkEndTime = s.WorkEndTime,
    })
    .ToList(),

                         // محاسباتی
                         AvgScoreForStylist = _context.RateHistories
                         .Where(x => x.StylistID == r.ID)
                         .Any()
                             ? _context.RateHistories.Where(x => x.StylistID == r.ID).Average(r => r.RateScore)
                             : 0,
                         RecommendPercent = _context.RateHistories
    .Where(x => x.StylistID == r.ID && x.RateQuestionID == 5)
    .Any()
        ? (
            _context.RateHistories.Count(x =>
                x.StylistID == r.ID &&
                x.RateQuestionID == 5 &&
                x.RateScore == 5.0
            ) * 100.0
            /
            _context.RateHistories.Count(x =>
                x.StylistID == r.ID &&
                x.RateQuestionID == 5
            )
          )
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
                     Specialty = stylistDTO.Result.Specialty,
                     StylistBio = stylistDTO.Result.StylistBio,
                     StylistName = stylistDTO.Result.StylistName,
                     StylistParentID = stylistDTO.Result.StylistParentID,
                     StylistServices = stylistDTO.Result.StylistServices,
                     WorkShopDepositAmount = stylistDTO.Result.WorkShopDepositAmount,
                     WorkShopInteractMode = stylistDTO.Result.WorkShopInteractMode,
                     WorkShopRentAmount = stylistDTO.Result.WorkShopRentAmount,
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
