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
    public class WorkerRep : IWorkerRep
    {

        private NobatPlusContext _context;
        public WorkerRep(NobatPlusContext context)
        {
            _context = context;
        }

        public async Task<BitResultObject> AddWorkerAsync(Worker Worker)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var duplicateProfile = await _context.Workers.AsNoTracking().AnyAsync(x =>
                    x.PersonID == Worker.PersonID &&
                    x.IsWorkShop == Worker.IsWorkShop);
                if (duplicateProfile)
                {
                    result.Status = false;
                    result.ErrorMessage = Worker.IsWorkShop
                        ? "برای این کاربر قبلاً پروفایل محل خدمت ثبت شده است"
                        : "برای این کاربر قبلاً پروفایل خدمات دهنده ثبت شده است";
                    return result;
                }

                await _context.Workers.AddAsync(Worker);
                await _context.SaveChangesAsync();
                result.ID = Worker.ID;
                _context.Entry(Worker).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<BitResultObject> EditWorkerAsync(Worker Worker)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var duplicateProfile = await _context.Workers.AsNoTracking().AnyAsync(x =>
                    x.ID != Worker.ID &&
                    x.PersonID == Worker.PersonID &&
                    x.IsWorkShop == Worker.IsWorkShop);
                if (duplicateProfile)
                {
                    result.Status = false;
                    result.ErrorMessage = Worker.IsWorkShop
                        ? "برای این کاربر قبلاً پروفایل محل خدمت ثبت شده است"
                        : "برای این کاربر قبلاً پروفایل خدمات دهنده ثبت شده است";
                    return result;
                }

                _context.Workers.Update(Worker);
                await _context.SaveChangesAsync();
                result.ID = Worker.ID;
                _context.Entry(Worker).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<BitResultObject> ExistWorkerAsync(string fieldValue, string fieldName)
        {
            BitResultObject result = new BitResultObject();
            long workerId = 0;
            try
            {
                switch (fieldName.ToLower().Trim())
                {
                    case "personid":
                        {
                            var theWorker = await _context.Workers.AsNoTracking().FirstOrDefaultAsync(x => x.PersonID == long.Parse(fieldValue)) ?? new Worker();
                            workerId = theWorker.ID;
                            break;
                        }
                    case "workerid":
                    default:
                        {
                            var theWorker = await _context.Workers.AsNoTracking().FirstOrDefaultAsync(x => x.ID == long.Parse(fieldValue)) ?? new Worker();
                            workerId = theWorker.ID;
                            break;
                        }
                }
                result.ID = workerId;
                result.Status = workerId > 0;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<ListResultObject<WorkerProfileDTO>> GetWorkerProfilesByPersonIdAsync(long personId)
        {
            var result = new ListResultObject<WorkerProfileDTO>();
            try
            {
                result.Results = await _context.Workers.Include(x=> x.JobType)
                    .AsNoTracking()
                    .Where(x => x.PersonID == personId)
                    .OrderByDescending(x => x.IsWorkShop)
                    .ThenBy(x => x.ID)
                    .Select(x => new WorkerProfileDTO
                    {
                        ID = x.ID,
                        PersonID = x.PersonID,
                        WorkerParentID = x.WorkerParentID,
                        Name = x.WorkerName,
                        ProfileType = x.IsWorkShop ? nameof(DbTools.BaseRole.WorkShop).ToString(): nameof(DbTools.BaseRole.Worker),
                        IsActive = x.IsActive,
                        AccountStatus = x.AccountStatus,
                        JobTypeId= x.JobTypeID,
                        JobTitle = x.JobType.JobTitle
                    })
                    .ToListAsync();
                result.TotalCount = result.Results.Count;
                result.PageCount = result.TotalCount > 0 ? 1 : 0;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }

            return result;
        }

        public async Task<ListResultObject<WorkerDTO>> GetAllWorkersAsync(
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

            var results = new ListResultObject<WorkerDTO>();

            try
            {
                IQueryable<Worker> query = _context.Workers.AsQueryable();

                if (parentId > 0)
                {
                    query = query.Where(x => x.WorkerParentID == parentId);
                }
                else if (parentId < 0 && parentId >= -10)
                {
                    query = query.Where(x => x.WorkerParentID > 0 || !x.IsWorkShop);
                }
                else if (parentId < -10)
                {
                    query = query.Where(x => x.WorkerParentID == 0 && x.IsWorkShop);
                }

                if (serviceIds.Count > 0)
                {
                    query = query.Where(st =>
                        st.WorkerServices.Any(ss =>
                            serviceIds.Contains(ss.ServiceManagementID)));
                }

                if (jobTypeId > 0)
                {
                    query = query.Where(x => x.JobTypeID == jobTypeId);
                }

                if (discountId > 0)
                {
                    var workerIds =
                        _context.DiscountAssignments
                            .Where(d => d.DiscountId == discountId && d.WorkerId != null)
                            .Select(d => d.WorkerId!.Value)
                        .Union(
                            _context.ServiceDiscounts
                                .Where(d => d.DiscountId == discountId && d.WorkerId != null)
                                .Select(d => d.WorkerId!.Value))
                        .Union(
                            _context.CustomerDiscounts
                                .Where(d => d.DiscountId == discountId && d.WorkerId != null)
                                .Select(d => d.WorkerId))
                        .Distinct();

                    query = query.Where(st => workerIds.Contains(st.ID));
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
                        st.WorkerServices.Any(ss => ss.ServicePrice >= fromPrice));
                }

                if (toPrice > 0)
                {
                    query = query.Where(st =>
                        st.WorkerServices.Any(ss => ss.ServicePrice <= toPrice));
                }

                query = query
                    .Include(x => x.Person)
                        .ThenInclude(x => x.Address)
                            .ThenInclude(x => x.City)
                    .Include(x => x.JobType)
                    .Include(x => x.WorkerServices)
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
                                ? x.WorkerServices.Any(s =>
                                    s.ServiceManagement.ServiceName.Contains(tokens[0]))
                                : tokens.All(token =>
                                    x.WorkerServices.Any(s =>
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
                    .Select(r => new WorkerDTO
                    {
                        ID = r.ID,
                        Description = r.Description ?? "",
                        IsActive = r.IsActive,
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
                        WorkerBio = r.WorkerBio ?? "",
                        
                        WorkerName = r.WorkerParentID > 0
                            ? _context.Workers
                                .Where(x => x.ID == r.WorkerParentID)
                                .Select(x => x.WorkerName)
                                .FirstOrDefault()
                            : r.WorkerName,

                        WorkerParentID = r.WorkerParentID,
                        WorkShopDepositAmount = r.WorkShopDepositAmount,
                        WorkShopInteractMode = r.WorkShopInteractMode ?? "",
                        WorkShopRentAmount = r.WorkShopRentAmount,
                        YearsOfExperience = r.YearsOfExperience,
                        RestTime = r.RestTime,
                        SlotIntervalMinutes = r.SlotIntervalMinutes,
                        BookingCreationMode = r.BookingCreationMode ?? "automatic",
                        SlotDisplayMode = r.SlotDisplayMode ?? "all",
                        
                        WorkerImagePath =
                            _context.Images.Any(x =>
                                x.EntityType.ToLower() == "worker" &&
                                x.ForeignKeyId == r.ID)
                                ? $"{_context.Settings.FirstOrDefault(x => x.Key.ToLower() == "apiurl").Value}/FileCenter/downloadfile?fileType=images&rowId=0&foreignkeyId={r.ID}&entityName=worker"
                                : "",

                        WorkerServices = r.WorkerServices.Where(ss => ss.ServiceManagement.ServiceParentID == 0)
                            .Select(s => new WorkerService
                            {
                                WorkerID = s.WorkerID,
                                ServiceManagementID = s.ServiceManagementID,
                                ServicePrice = s.ServicePrice,
                                ServiceDuration = s.ServiceDuration,
                                DepositPercent = s.DepositPercent,
                                HasDynamicPricing = s.HasDynamicPricing,
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

                        AvgScoreForWorker = _context.RateHistories
                            .Where(x => x.WorkerID == r.ID)
                            .Any()
                                ? _context.RateHistories
                                    .Where(x => x.WorkerID == r.ID)
                                    .Average(rr => rr.RateScore)
                                : 0,

                        RecommendPercent = _context.RateHistories
                            .Where(x => x.WorkerID == r.ID && x.RateQuestionID == 5)
                            .Any()
                                ? (
                                    _context.RateHistories.Count(x =>
                                        x.WorkerID == r.ID &&
                                        x.RateQuestionID == 5 &&
                                        x.RateScore == 5.0) * 100.0
                                    /
                                    _context.RateHistories.Count(x =>
                                        x.WorkerID == r.ID &&
                                        x.RateQuestionID == 5)
                                  )
                                : 0,

                        TodayBookingsCount = _context.Bookings
                            .Count(b =>
                                b.WorkerID == r.ID &&
                                b.BookingDate.Date == DateTime.Today),

                        TotalBookingsCount = _context.Bookings
                            .Count(b => b.WorkerID == r.ID),

                        // اینجا دیگر داخل Query اصلی محاسبه نمی‌کنیم
                        SuccededBokingCount = 0,

                        WorkshopWorkerCount = _context.Workers
                            .Count(b => b.WorkerParentID == r.ID),

                        TotalCustomersCount = _context.Bookings
                            .Where(b => b.WorkerID == r.ID)
                            .Select(b => b.CustomerID)
                            .Distinct()
                            .Count(),

                        IsOnLeaveNow = _context.WorkerPacifics
                            .Any(p =>
                                p.WorkerID == r.ID &&
                                DateTime.Now >= p.PacificStartDate &&
                                DateTime.Now <= p.PacificEndDate)
                    })
                    .SortBy(sortQuery)
                    .ToListAsync();

                // اینجا مقدار نوبت‌های موفق خودش + همه زیرمجموعه‌ها را پر می‌کنیم
                await FillSuccededBookingCountsAsync(results.Results);
                await FillRootWorkshopServicesAsync(results.Results);
            }
            catch (Exception ex)
            {
                results.Status = false;
                results.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }

            return results;
        }

        public async Task<RowResultObject<WorkerDTO>> GetWorkerByIdAsync(long WorkerId)
        {
            RowResultObject<WorkerDTO> result = new RowResultObject<WorkerDTO>();

            try
            {
                result.Result = await _context.Workers
                    .AsNoTracking()
                    .Include(x => x.Person)
                        .ThenInclude(x => x.Address)
                            .ThenInclude(x => x.City)
                    .Include(x => x.JobType)
                    .Include(x => x.WorkerServices)
                        .ThenInclude(x => x.ServiceManagement)
                    .Include(x => x.WorkTimes)
                    .Include(x => x.SocialNetworks)
                    .Where(x => x.ID == WorkerId)
                    .Select(r => new WorkerDTO
                    {
                        ID = r.ID,
                        Description = r.Description ?? "",
                        IsActive = r.IsActive,
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
                        WorkerBio = r.WorkerBio ?? "",

                        WorkerName = r.WorkerParentID > 0
                            ? _context.Workers
                                .Where(x => x.ID == r.WorkerParentID)
                                .Select(x => x.WorkerName)
                                .FirstOrDefault()
                            : r.WorkerName,

                        WorkerParentID = r.WorkerParentID,
                        WorkShopDepositAmount = r.WorkShopDepositAmount,
                        WorkShopInteractMode = r.WorkShopInteractMode ?? "",
                        WorkShopRentAmount = r.WorkShopRentAmount,
                        YearsOfExperience = r.YearsOfExperience,
                        RestTime = r.RestTime,
                        SlotIntervalMinutes = r.SlotIntervalMinutes,
                        BookingCreationMode = r.BookingCreationMode ?? "automatic",
                        SlotDisplayMode = r.SlotDisplayMode ?? "all",

                        WorkerImagePath = _context.Images.Any(x =>
                            x.EntityType.ToLower() == "worker" &&
                            x.ForeignKeyId == r.ID)
                                ? $"{_context.Settings.FirstOrDefault(x=> x.Key.ToLower()== "apiurl").Value}/FileCenter/downloadfile?fileType=images&rowId=0&foreignkeyId={r.ID}&entityName=worker"
                                : "",

                        WorkerServices = r.WorkerServices.Where(ss => ss.ServiceManagement.ServiceParentID == 0)
                            .Select(s => new WorkerService
                            {
                                WorkerID = s.WorkerID,
                                ServiceManagementID = s.ServiceManagementID,
                                ServicePrice = s.ServicePrice,
                                ServiceDuration = s.ServiceDuration,
                                DepositPercent = s.DepositPercent,
                                HasDynamicPricing = s.HasDynamicPricing,
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

                        AvgScoreForWorker = _context.RateHistories
                            .Where(x => x.WorkerID == r.ID)
                            .Any()
                                ? _context.RateHistories
                                    .Where(x => x.WorkerID == r.ID)
                                    .Average(x => x.RateScore)
                                : 0,

                        RecommendPercent = _context.RateHistories
                            .Where(x => x.WorkerID == r.ID && x.RateQuestionID == 5)
                            .Any()
                                ? (
                                    _context.RateHistories.Count(x =>
                                        x.WorkerID == r.ID &&
                                        x.RateQuestionID == 5 &&
                                        x.RateScore == 5.0
                                    ) * 100.0
                                    /
                                    _context.RateHistories.Count(x =>
                                        x.WorkerID == r.ID &&
                                        x.RateQuestionID == 5
                                    )
                                  )
                                : 0,

                        TodayBookingsCount = _context.Bookings
                            .Count(b =>
                                b.WorkerID == r.ID &&
                                b.BookingDate.Date == DateTime.Today),

                        // اینجا دیگر محاسبه نمی‌کنیم چون نیاز به زیرمجموعه‌ها دارد
                        SuccededBokingCount = 0,

                        WorkshopWorkerCount = _context.Workers
                            .Count(b => b.WorkerParentID == r.ID),

                        TotalBookingsCount = _context.Bookings
                            .Count(b => b.WorkerID == r.ID),

                        TotalCustomersCount = _context.Bookings
                            .Where(b => b.WorkerID == r.ID)
                            .Select(b => b.CustomerID)
                            .Distinct()
                            .Count(),
                        
                        IsOnLeaveNow = _context.WorkerPacifics
                            .Any(p =>
                                p.WorkerID == r.ID &&
                                DateTime.Now >= p.PacificStartDate &&
                                DateTime.Now <= p.PacificEndDate)
                    })
                    .SingleOrDefaultAsync();

                if (result.Result != null)
                {
                    result.Result.SuccededBokingCount =
                        await GetSucceededBookingCountWithChildrenAsync(result.Result.ID);

                    await FillRootWorkshopServicesAsync(new List<WorkerDTO> { result.Result });
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }

            return result;
        }

        public async Task<BitResultObject> RemoveWorkerAsync(Worker Worker)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.Workers.Remove(Worker);
                await _context.SaveChangesAsync();
                result.ID = Worker.ID;
                _context.Entry(Worker).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
           
        }

        public async Task<BitResultObject> RemoveWorkerAsync(long WorkerId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var workerDTO = await GetWorkerByIdAsync(WorkerId);
                var worker = new Worker()
                 {
                     ID = workerDTO.Result.ID,
                     Description = workerDTO.Result.Description,
                     UpdateDate = workerDTO.Result.UpdateDate,
                     CreateDate = workerDTO.Result.CreateDate,
                     AccountStatus = workerDTO.Result.AccountStatus,
                     PayMethod = workerDTO.Result.PayMethod,
                     Bookings = workerDTO.Result.Bookings,
                     CustomerDiscounts = workerDTO.Result.CustomerDiscounts,
                     IsWorkShop = workerDTO.Result.IsWorkShop,
                     GenderAccepted = workerDTO.Result.GenderAccepted,
                     DiscountAssignments = workerDTO.Result.DiscountAssignments,
                     JobType = workerDTO.Result.JobType,
                     JobTypeID = workerDTO.Result.JobTypeID,
                     Person = workerDTO.Result.Person,
                     PersonID = workerDTO.Result.PersonID,
                     RateHistories = workerDTO.Result.RateHistories,
                     ServiceDiscounts = workerDTO.Result.ServiceDiscounts,
                     Specialty = workerDTO.Result.Specialty,
                     WorkerBio = workerDTO.Result.WorkerBio,
                     WorkerName = workerDTO.Result.WorkerName,
                     WorkerParentID = workerDTO.Result.WorkerParentID,
                     WorkerServices = workerDTO.Result.WorkerServices,
                     WorkShopDepositAmount = workerDTO.Result.WorkShopDepositAmount,
                     WorkShopInteractMode = workerDTO.Result.WorkShopInteractMode,
                     WorkShopRentAmount = workerDTO.Result.WorkShopRentAmount,
                     YearsOfExperience = workerDTO.Result.YearsOfExperience,
                     RestTime = workerDTO.Result.RestTime,
                     SlotIntervalMinutes = workerDTO.Result.SlotIntervalMinutes,
                     BookingCreationMode = workerDTO.Result.BookingCreationMode,
                     SlotDisplayMode = workerDTO.Result.SlotDisplayMode,
                 };
                result = await RemoveWorkerAsync(worker);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
          
        }

        #region HelperFuncs

        private async Task FillRootWorkshopServicesAsync(ICollection<WorkerDTO> workers)
        {
            if (workers == null || workers.Count == 0)
                return;

            var rootWorkshopIds = workers
                .Where(x => x.WorkerParentID == 0 && x.IsWorkShop)
                .Select(x => x.ID)
                .Distinct()
                .ToList();

            if (rootWorkshopIds.Count == 0)
                return;

            var childServiceRows = await _context.WorkerServices.Where(ss => ss.ServiceManagement.ServiceParentID == 0)
                .AsNoTracking()
                .Where(service => rootWorkshopIds.Contains(service.Worker.WorkerParentID))
                .Select(service => new
                {
                    WorkshopID = service.Worker.WorkerParentID,
                    service.WorkerID,
                    service.ServiceManagementID,
                    service.ServicePrice,
                    service.ServiceDuration,
                    service.DepositPercent,
                    service.HasDynamicPricing,
                    service.ServiceManagement
                })
                .ToListAsync();

            var servicesByWorkshop = childServiceRows
                .GroupBy(x => x.WorkshopID)
                .ToDictionary(
                    group => group.Key,
                    group => (ICollection<WorkerService>)group
                        .Select(service => new WorkerService
                        {
                            WorkerID = service.WorkerID,
                            ServiceManagementID = service.ServiceManagementID,
                            ServicePrice = service.ServicePrice,
                            ServiceDuration = service.ServiceDuration,
                            DepositPercent = service.DepositPercent,
                            HasDynamicPricing = service.HasDynamicPricing,
                            ServiceManagement = service.ServiceManagement
                        })
                        .ToList());

            foreach (var workshop in workers.Where(x => rootWorkshopIds.Contains(x.ID)))
            {
                workshop.WorkerServices = servicesByWorkshop.TryGetValue(workshop.ID, out var services)
                    ? services
                    : new List<WorkerService>();
            }
        }

        private async Task FillSuccededBookingCountsAsync(List<WorkerDTO> workers)
        {
            if (workers == null || workers.Count == 0)
                return;

            var rootWorkerIds = workers
                .Select(x => x.ID)
                .Distinct()
                .ToList();

            var workerLinks = await _context.Workers
                .AsNoTracking()
                .Select(x => new
                {
                    x.ID,
                    x.WorkerParentID
                })
                .ToListAsync();

            var childrenLookup = workerLinks
                .Where(x => x.WorkerParentID > 0)
                .GroupBy(x => x.WorkerParentID)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.ID).ToList()
                );

            var relatedWorkerIds = new HashSet<long>();

            foreach (var rootId in rootWorkerIds)
            {
                CollectWorkerAndChildrenIds(rootId, childrenLookup, relatedWorkerIds);
            }

            var bookingCounts = await _context.Bookings
                .AsNoTracking()
                .Where(b =>
                    relatedWorkerIds.Contains(b.WorkerID) &&
                    !b.IsCancelled)
                .GroupBy(b => b.WorkerID)
                .Select(g => new
                {
                    WorkerID = g.Key,
                    Count = g.Count()
                })
                .ToDictionaryAsync(x => x.WorkerID, x => x.Count);

            var countCache = new Dictionary<long, int>();

            int GetTotalSucceededBookingCount(long workerId)
            {
                if (countCache.TryGetValue(workerId, out var cachedCount))
                    return cachedCount;

                var total = bookingCounts.TryGetValue(workerId, out var ownCount)
                    ? ownCount
                    : 0;

                if (childrenLookup.TryGetValue(workerId, out var childIds))
                {
                    foreach (var childId in childIds)
                    {
                        total += GetTotalSucceededBookingCount(childId);
                    }
                }

                countCache[workerId] = total;
                return total;
            }

            foreach (var worker in workers)
            {
                worker.SuccededBokingCount = GetTotalSucceededBookingCount(worker.ID);
            }
        }

        private void CollectWorkerAndChildrenIds(
    long workerId,
    Dictionary<long, List<long>> childrenLookup,
    HashSet<long> result)
        {
            if (!result.Add(workerId))
                return;

            if (!childrenLookup.TryGetValue(workerId, out var childIds))
                return;

            foreach (var childId in childIds)
            {
                CollectWorkerAndChildrenIds(childId, childrenLookup, result);
            }
        }

        private async Task<int> GetSucceededBookingCountWithChildrenAsync(long workerId)
        {
            var workerIds = await GetWorkerAndAllChildrenIdsAsync(workerId);

            if (workerIds.Count == 0)
                return 0;

            return await _context.Bookings
                .AsNoTracking()
                .CountAsync(b =>
                    workerIds.Contains(b.WorkerID) &&
                    !b.IsCancelled);
        }

        private async Task<List<long>> GetWorkerAndAllChildrenIdsAsync(long workerId)
        {
            var result = new HashSet<long> { workerId };

            var currentLevelIds = new List<long> { workerId };

            while (currentLevelIds.Count > 0)
            {
                var childIds = await _context.Workers
                    .AsNoTracking()
                    .Where(s => currentLevelIds.Contains(s.WorkerParentID))
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
