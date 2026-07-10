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
     FindLocationRequestBody findLocation = null,
     string accountStatus = "")
        {
            serviceIds ??= new List<long>();

            var results = new ListResultObject<StylistDTO>();

            try
            {
                IQueryable<Stylist> query = _context.Stylists.AsQueryable();

                if (parentId > 0)
                {
                    query = query.Where(x => x.StylistParentID == parentId);
                }
                else if (parentId < 0 && parentId >= -10)
                {
                    query = query.Where(x => x.StylistParentID > 0 || !x.IsWorkShop);
                }
                else if (parentId < -10)
                {
                    query = query.Where(x => x.StylistParentID == 0 && x.IsWorkShop);
                }

                if (serviceIds.Count > 0)
                {
                    query = query.Where(st =>
                        st.StylistServices.Any(ss =>
                            serviceIds.Contains(ss.ServiceManagementID)));
                }

                if (jobTypeId > 0)
                {
                    query = query.Where(x => x.JobTypeID == jobTypeId);
                }

                if (discountId > 0)
                {
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

                if (cityId > 0)
                {
                    query = query.Where(x =>
                        x.Person != null &&
                        x.Person.Address != null &&
                        x.Person.Address.CityID == cityId);
                }

                if (gender > 0)
                {
                    query = query.Where(x =>
                        x.Person != null &&
                        x.Person.Gender == gender);
                }

                if (!string.IsNullOrWhiteSpace(accountStatus))
                {
                    accountStatus = accountStatus.Trim();
                    query = query.Where(x => x.AccountStatus == accountStatus);
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

                query = query
                    .Include(x => x.Person)
                        .ThenInclude(x => x.Address)
                            .ThenInclude(x => x.City)
                    .Include(x => x.JobType)
                    .Include(x => x.StylistServices)
                        .ThenInclude(x => x.ServiceManagement)
                    .Include(x => x.WorkTimes)
                    .Include(x => x.SocialNetworks)
                    .AsNoTracking();

                if (findLocation != null && findLocation.RadiusKm > 0)
                {
                    double personLat = 0;
                    double personLng = 0;

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

                        (
                            x.Person.FirstName != null &&
                            x.Person.LastName != null &&
                            (
                                (x.Person.FirstName + " " + x.Person.LastName).Contains(searchText) ||
                                (x.Person.FirstName + x.Person.LastName).Contains(searchText)
                            )
                        ) ||

                        (x.Person.NaCode != null && x.Person.NaCode.Contains(searchText)) ||
                        (x.Person.PhoneNumber != null && x.Person.PhoneNumber.Contains(searchText)) ||
                        (x.Person.Email != null && x.Person.Email.Contains(searchText)) ||
                        (x.Person.Description != null && x.Person.Description.Contains(searchText)) ||

                        (
                            x.Person.Address != null &&
                            x.Person.Address.City != null &&
                            x.Person.Address.City.CityName != null &&
                            x.Person.Address.City.CityName.Contains(searchText)
                        ) ||

                        (x.JobType != null && x.JobType.JobTitle != null && x.JobType.JobTitle.Contains(searchText)) ||
                        (x.Specialty != null && x.Specialty.Contains(searchText)) ||

                        (
                            tokens.Count == 1
                                ? x.StylistServices.Any(s =>
                                    s.ServiceManagement.ServiceName.Contains(tokens[0]))
                                : tokens.All(token =>
                                    x.StylistServices.Any(s =>
                                        s.ServiceManagement.ServiceName.Contains(token)))
                        ) ||

                        (x.Description != null && x.Description.Contains(searchText))
                    );
                }

                results.TotalCount = await query.CountAsync();
                results.PageCount = DbTools.GetPageCount(results.TotalCount, pageSize);

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
                        
                        StylistName = r.StylistParentID > 0
                            ? _context.Stylists
                                .Where(x => x.ID == r.StylistParentID)
                                .Select(x => x.StylistName)
                                .FirstOrDefault()
                            : r.StylistName,

                        StylistParentID = r.StylistParentID,
                        WorkShopDepositAmount = r.WorkShopDepositAmount,
                        WorkShopInteractMode = r.WorkShopInteractMode ?? "",
                        WorkShopRentAmount = r.WorkShopRentAmount,
                        YearsOfExperience = r.YearsOfExperience,
                        RestTime = r.RestTime,

                        StylistImagePath =
                            _context.Images.Any(x =>
                                x.EntityType.ToLower() == "stylist" &&
                                x.ForeignKeyId == r.ID)
                                ? $"{_context.Settings.FirstOrDefault(x => x.Key.ToLower() == "apiurl").Value}/FileCenter/downloadfile?fileType=images&rowId=0&foreignkeyId={r.ID}&entityName=stylist"
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
                                ? _context.RateHistories
                                    .Where(x => x.StylistID == r.ID)
                                    .Average(rr => rr.RateScore)
                                : 0,

                        RecommendPercent = _context.RateHistories
                            .Where(x => x.StylistID == r.ID && x.RateQuestionID == 5)
                            .Any()
                                ? (
                                    _context.RateHistories.Count(x =>
                                        x.StylistID == r.ID &&
                                        x.RateQuestionID == 5 &&
                                        x.RateScore == 5.0) * 100.0
                                    /
                                    _context.RateHistories.Count(x =>
                                        x.StylistID == r.ID &&
                                        x.RateQuestionID == 5)
                                  )
                                : 0,

                        TodayBookingsCount = _context.Bookings
                            .Count(b =>
                                b.StylistID == r.ID &&
                                b.BookingDate.Date == DateTime.Today),

                        TotalBookingsCount = _context.Bookings
                            .Count(b => b.StylistID == r.ID),

                        // اینجا دیگر داخل Query اصلی محاسبه نمی‌کنیم
                        SuccededBokingCount = 0,

                        SalonStylistCount = _context.Stylists
                            .Count(b => b.StylistParentID == r.ID),

                        TotalCustomersCount = _context.Bookings
                            .Where(b => b.StylistID == r.ID)
                            .Select(b => b.CustomerID)
                            .Distinct()
                            .Count(),

                        IsOnLeaveNow = _context.StylistPacifics
                            .Any(p =>
                                p.StylistID == r.ID &&
                                DateTime.Now >= p.PacificStartDate &&
                                DateTime.Now <= p.PacificEndDate)
                    })
                    .SortBy(sortQuery)
                    .ToListAsync();

                // اینجا مقدار نوبت‌های موفق خودش + همه زیرمجموعه‌ها را پر می‌کنیم
                await FillSuccededBookingCountsAsync(results.Results);
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
                result.Result = await _context.Stylists
                    .AsNoTracking()
                    .Include(x => x.Person)
                        .ThenInclude(x => x.Address)
                            .ThenInclude(x => x.City)
                    .Include(x => x.JobType)
                    .Include(x => x.StylistServices)
                        .ThenInclude(x => x.ServiceManagement)
                    .Include(x => x.WorkTimes)
                    .Include(x => x.SocialNetworks)
                    .Where(x => x.ID == StylistId)
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

                        StylistName = r.StylistParentID > 0
                            ? _context.Stylists
                                .Where(x => x.ID == r.StylistParentID)
                                .Select(x => x.StylistName)
                                .FirstOrDefault()
                            : r.StylistName,

                        StylistParentID = r.StylistParentID,
                        WorkShopDepositAmount = r.WorkShopDepositAmount,
                        WorkShopInteractMode = r.WorkShopInteractMode ?? "",
                        WorkShopRentAmount = r.WorkShopRentAmount,
                        YearsOfExperience = r.YearsOfExperience,
                        RestTime = r.RestTime,

                        StylistImagePath = _context.Images.Any(x =>
                            x.EntityType.ToLower() == "stylist" &&
                            x.ForeignKeyId == r.ID)
                                ? $"{_context.Settings.FirstOrDefault(x=> x.Key.ToLower()== "apiurl").Value}/FileCenter/downloadfile?fileType=images&rowId=0&foreignkeyId={r.ID}&entityName=stylist"
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
                                WorkEndTime = s.WorkEndTime,
                            })
                            .ToList(),

                        AvgScoreForStylist = _context.RateHistories
                            .Where(x => x.StylistID == r.ID)
                            .Any()
                                ? _context.RateHistories
                                    .Where(x => x.StylistID == r.ID)
                                    .Average(x => x.RateScore)
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
                            .Count(b =>
                                b.StylistID == r.ID &&
                                b.BookingDate.Date == DateTime.Today),

                        // اینجا دیگر محاسبه نمی‌کنیم چون نیاز به زیرمجموعه‌ها دارد
                        SuccededBokingCount = 0,

                        SalonStylistCount = _context.Stylists
                            .Count(b => b.StylistParentID == r.ID),

                        TotalBookingsCount = _context.Bookings
                            .Count(b => b.StylistID == r.ID),

                        TotalCustomersCount = _context.Bookings
                            .Where(b => b.StylistID == r.ID)
                            .Select(b => b.CustomerID)
                            .Distinct()
                            .Count(),

                        IsOnLeaveNow = _context.StylistPacifics
                            .Any(p =>
                                p.StylistID == r.ID &&
                                DateTime.Now >= p.PacificStartDate &&
                                DateTime.Now <= p.PacificEndDate)
                    })
                    .SingleOrDefaultAsync();

                if (result.Result != null)
                {
                    result.Result.SuccededBokingCount =
                        await GetSucceededBookingCountWithChildrenAsync(result.Result.ID);
                }
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

        #region HelperFuncs

        private async Task FillSuccededBookingCountsAsync(List<StylistDTO> stylists)
        {
            if (stylists == null || stylists.Count == 0)
                return;

            var rootStylistIds = stylists
                .Select(x => x.ID)
                .Distinct()
                .ToList();

            var stylistLinks = await _context.Stylists
                .AsNoTracking()
                .Select(x => new
                {
                    x.ID,
                    x.StylistParentID
                })
                .ToListAsync();

            var childrenLookup = stylistLinks
                .Where(x => x.StylistParentID > 0)
                .GroupBy(x => x.StylistParentID)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.ID).ToList()
                );

            var relatedStylistIds = new HashSet<long>();

            foreach (var rootId in rootStylistIds)
            {
                CollectStylistAndChildrenIds(rootId, childrenLookup, relatedStylistIds);
            }

            var bookingCounts = await _context.Bookings
                .AsNoTracking()
                .Where(b =>
                    relatedStylistIds.Contains(b.StylistID) &&
                    !b.IsCancelled)
                .GroupBy(b => b.StylistID)
                .Select(g => new
                {
                    StylistID = g.Key,
                    Count = g.Count()
                })
                .ToDictionaryAsync(x => x.StylistID, x => x.Count);

            var countCache = new Dictionary<long, int>();

            int GetTotalSucceededBookingCount(long stylistId)
            {
                if (countCache.TryGetValue(stylistId, out var cachedCount))
                    return cachedCount;

                var total = bookingCounts.TryGetValue(stylistId, out var ownCount)
                    ? ownCount
                    : 0;

                if (childrenLookup.TryGetValue(stylistId, out var childIds))
                {
                    foreach (var childId in childIds)
                    {
                        total += GetTotalSucceededBookingCount(childId);
                    }
                }

                countCache[stylistId] = total;
                return total;
            }

            foreach (var stylist in stylists)
            {
                stylist.SuccededBokingCount = GetTotalSucceededBookingCount(stylist.ID);
            }
        }

        private void CollectStylistAndChildrenIds(
    long stylistId,
    Dictionary<long, List<long>> childrenLookup,
    HashSet<long> result)
        {
            if (!result.Add(stylistId))
                return;

            if (!childrenLookup.TryGetValue(stylistId, out var childIds))
                return;

            foreach (var childId in childIds)
            {
                CollectStylistAndChildrenIds(childId, childrenLookup, result);
            }
        }

        private async Task<int> GetSucceededBookingCountWithChildrenAsync(long stylistId)
        {
            var stylistIds = await GetStylistAndAllChildrenIdsAsync(stylistId);

            if (stylistIds.Count == 0)
                return 0;

            return await _context.Bookings
                .AsNoTracking()
                .CountAsync(b =>
                    stylistIds.Contains(b.StylistID) &&
                    !b.IsCancelled);
        }

        private async Task<List<long>> GetStylistAndAllChildrenIdsAsync(long stylistId)
        {
            var result = new HashSet<long> { stylistId };

            var currentLevelIds = new List<long> { stylistId };

            while (currentLevelIds.Count > 0)
            {
                var childIds = await _context.Stylists
                    .AsNoTracking()
                    .Where(s => currentLevelIds.Contains(s.StylistParentID))
                    .Select(s => s.ID)
                    .ToListAsync();

                currentLevelIds = childIds
                    .Where(id => result.Add(id))
                    .ToList();
            }

            return result.ToList();
        }

        #endregion
    }
}
