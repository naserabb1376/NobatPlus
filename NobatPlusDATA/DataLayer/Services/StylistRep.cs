using Domain;
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

        public async Task<ListResultObject<StylistDTO>> GetAllStylistsAsync(long parentId = 0,long cityId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="")
        {
            ListResultObject<StylistDTO> results = new ListResultObject<StylistDTO>();
            try
            {
                IQueryable<Stylist> query = _context.Stylists.Include(x => x.Person).ThenInclude(x => x.Address).ThenInclude(x => x.City).Include(x => x.JobType).AsNoTracking();

                if (parentId >= 0)
                {
                    query = query.Where(x=> x.StylistParentID == parentId);
                }

                if (parentId < 0)
                {
                    query = query.Where(x => x.StylistParentID > 0);
                }

                query = query.Where(x =>
                       (!string.IsNullOrEmpty(x.Person.FirstName) && x.Person.FirstName.Contains(searchText)) ||
                       (!string.IsNullOrEmpty(x.Person.LastName) && x.Person.LastName.Contains(searchText)) ||
                       (!string.IsNullOrEmpty(x.Person.NaCode) && x.Person.NaCode.Contains(searchText)) ||
                       (!string.IsNullOrEmpty(x.Person.PhoneNumber) && x.Person.PhoneNumber.Contains(searchText)) ||
                       (!string.IsNullOrEmpty(x.Person.Email) && x.Person.Email.Contains(searchText)) ||
                       (!string.IsNullOrEmpty(x.Person.Description) && x.Person.Description.Contains(searchText)) ||
                       (!string.IsNullOrEmpty(x.Person.DateOfBirth.ToString()) && x.Person.DateOfBirth.ToString().Contains(searchText)) ||
                       (!string.IsNullOrEmpty(x.Person.Address.City.CityName.ToString()) && x.Person.Address.City.CityName.ToString().Contains(searchText)) ||
                       (!string.IsNullOrEmpty(x.Person.Address.AddressLocationHorizentalPoint.ToString()) && x.Person.Address.AddressLocationHorizentalPoint.ToString().Contains(searchText)) ||
                       (!string.IsNullOrEmpty(x.Person.Address.AddressLocationVerticalPoint.ToString()) && x.Person.Address.AddressLocationVerticalPoint.ToString().Contains(searchText)) ||
                       (!string.IsNullOrEmpty(x.Person.Address.AddressPostalCode.ToString()) && x.Person.Address.AddressPostalCode.ToString().Contains(searchText)) ||
                       //(!string.IsNullOrEmpty(x.State.ToString()) && x.State.ToString().Contains(searchText)) ||
                       (!string.IsNullOrEmpty(x.Person.Address.Description.ToString()) && x.Person.Address.Description.ToString().Contains(searchText)) ||
                       (!string.IsNullOrEmpty(x.Person.Address.AddressStreet.ToString()) && x.Person.Address.AddressStreet.ToString().Contains(searchText)) ||
                       (!string.IsNullOrEmpty(x.JobType.JobTitle) && x.JobType.JobTitle.Contains(searchText)) ||
                       (x.CreateDate.HasValue && x.CreateDate.Value.ToString().Contains(searchText)) ||
                       (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString().Contains(searchText)) ||
                       (!string.IsNullOrEmpty(x.YearsOfExperience.ToString()) && x.YearsOfExperience.ToString().Contains(searchText)) ||
                       (!string.IsNullOrEmpty(x.Specialty) && x.Specialty.Contains(searchText)) ||
                       (!string.IsNullOrEmpty(x.StylistBio) && x.StylistBio.Contains(searchText)) ||
                       (!string.IsNullOrEmpty(x.StylistName) && x.StylistName.Contains(searchText)) ||
                       (!string.IsNullOrEmpty(x.WorkShopInteractMode) && x.WorkShopInteractMode.Contains(searchText)) ||
                       (!string.IsNullOrEmpty(x.AccountStatus) && x.AccountStatus.Contains(searchText)) ||
                       (!string.IsNullOrEmpty(x.PayMethod) && x.PayMethod.Contains(searchText)) ||
                       (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText))
                   );

                results.TotalCount = query.Count();
                results.PageCount = DbTools.GetPageCount(results.TotalCount, pageSize);
                results.Results = await query.OrderByDescending(x => x.CreateDate)
                .SortBy(sortQuery).ToPaging(pageIndex, pageSize)
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

                    // محاسباتی
                    AvgScoreForStylist = _context.RateHistories
                         .Where(x => x.StylistID == r.ID)
                         .Any()
                             ? _context.RateHistories.Where(x => x.StylistID == r.ID).Average(r => r.RateScore)
                             : 0
                }).ToListAsync();
            }
            catch (Exception ex)
            {
                results.Status = false;
                results.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return results;
           
        }

        public async Task<ListResultObject<StylistDTO>> GetStylistsOfServiceAsync(long serviceManagementId,long cityId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="")
        {
            ListResultObject<StylistDTO> results = new ListResultObject<StylistDTO>();
            try
            {
                var query = _context.StylistServices
                .Where(bs => bs.ServiceManagementID == serviceManagementId)
                .Select(bs => bs.Stylist)
                .Include(x => x.Person).ThenInclude(x => x.Address).ThenInclude(x => x.City)
                .Include(x => x.JobType)
                .AsNoTracking()
                .Where(x =>
                    (!string.IsNullOrEmpty(x.Person.FirstName) && x.Person.FirstName.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.LastName) && x.Person.LastName.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.NaCode) && x.Person.NaCode.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.PhoneNumber) && x.Person.PhoneNumber.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Email) && x.Person.Email.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Description) && x.Person.Description.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.DateOfBirth.ToString()) && x.Person.DateOfBirth.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.JobType.JobTitle) && x.JobType.JobTitle.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Address.City.CityName.ToString()) && x.Person.Address.City.CityName.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Address.AddressLocationHorizentalPoint.ToString()) && x.Person.Address.AddressLocationHorizentalPoint.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Address.AddressLocationVerticalPoint.ToString()) && x.Person.Address.AddressLocationVerticalPoint.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Address.AddressPostalCode.ToString()) && x.Person.Address.AddressPostalCode.ToString().Contains(searchText)) ||
                    //(!string.IsNullOrEmpty(x.State.ToString()) && x.State.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Address.Description.ToString()) && x.Person.Address.Description.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Address.AddressStreet.ToString()) && x.Person.Address.AddressStreet.ToString().Contains(searchText)) ||
                    (x.CreateDate.HasValue && x.CreateDate.Value.ToString().Contains(searchText)) ||
                    (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.YearsOfExperience.ToString()) && x.YearsOfExperience.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Specialty) && x.Specialty.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText))
                );

                results.TotalCount = query.Count();
                results.PageCount = DbTools.GetPageCount(results.TotalCount, pageSize);
                results.Results = await query.OrderByDescending(x => x.CreateDate)
                .SortBy(sortQuery).ToPaging(pageIndex, pageSize)
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

                    // محاسباتی
                    AvgScoreForStylist = _context.RateHistories
                         .Where(x => x.StylistID == r.ID)
                         .Any()
                             ? _context.RateHistories.Where(x => x.StylistID == r.ID).Average(r => r.RateScore)
                             : 0
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

        public async Task<ListResultObject<StylistDTO>> GetStylistsOfJobTypeAsync(long JobTypeId,long cityId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="")
        {
            ListResultObject<StylistDTO> results = new ListResultObject<StylistDTO>();
            try
            {
                var query = _context.Stylists
                .Include(x => x.Person).ThenInclude(x => x.Address).ThenInclude(x => x.City)
                .Include(x => x.JobType)
                .AsNoTracking()
                .Where(x =>
                    x.JobTypeID == JobTypeId &&
                    ((!string.IsNullOrEmpty(x.Person.FirstName) && x.Person.FirstName.Contains(searchText)) ||
                     (!string.IsNullOrEmpty(x.Person.LastName) && x.Person.LastName.Contains(searchText)) ||
                     (!string.IsNullOrEmpty(x.Person.NaCode) && x.Person.NaCode.Contains(searchText)) ||
                     (!string.IsNullOrEmpty(x.Person.PhoneNumber) && x.Person.PhoneNumber.Contains(searchText)) ||
                     (!string.IsNullOrEmpty(x.Person.Email) && x.Person.Email.Contains(searchText)) ||
                     (!string.IsNullOrEmpty(x.Person.Description) && x.Person.Description.Contains(searchText)) ||
                     (!string.IsNullOrEmpty(x.Person.DateOfBirth.ToString()) && x.Person.DateOfBirth.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Address.City.CityName.ToString()) && x.Person.Address.City.CityName.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Address.AddressLocationHorizentalPoint.ToString()) && x.Person.Address.AddressLocationHorizentalPoint.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Address.AddressLocationVerticalPoint.ToString()) && x.Person.Address.AddressLocationVerticalPoint.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Address.AddressPostalCode.ToString()) && x.Person.Address.AddressPostalCode.ToString().Contains(searchText)) ||
                    //(!string.IsNullOrEmpty(x.State.ToString()) && x.State.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Address.Description.ToString()) && x.Person.Address.Description.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Address.AddressStreet.ToString()) && x.Person.Address.AddressStreet.ToString().Contains(searchText)) ||
                     (!string.IsNullOrEmpty(x.JobType.JobTitle) && x.JobType.JobTitle.Contains(searchText)) ||
                     (x.CreateDate.HasValue && x.CreateDate.Value.ToString().Contains(searchText)) ||
                     (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString().Contains(searchText)) ||
                     (!string.IsNullOrEmpty(x.YearsOfExperience.ToString()) && x.YearsOfExperience.ToString().Contains(searchText)) ||
                     (!string.IsNullOrEmpty(x.Specialty) && x.Specialty.Contains(searchText))
                    )
                );

                results.TotalCount = query.Count();
                results.PageCount = DbTools.GetPageCount(results.TotalCount, pageSize);
                results.Results = await query.OrderByDescending(x => x.CreateDate)
                .SortBy(sortQuery).ToPaging(pageIndex, pageSize)
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

                     // محاسباتی
                     AvgScoreForStylist = _context.RateHistories
                         .Where(x => x.StylistID == r.ID)
                         .Any()
                             ? _context.RateHistories.Where(x => x.StylistID == r.ID).Average(r => r.RateScore)
                             : 0
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

        public async Task<ListResultObject<StylistDTO>> GetStylistsOfDiscountAsync(long DiscountId,long cityId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="")
        {
            ListResultObject<StylistDTO> results = new ListResultObject<StylistDTO>();
            try
            {
                var discountAssignments = _context.DiscountAssignments
               .Where(bs => bs.DiscountId == DiscountId)
               .Select(bs => bs.Stylist)
               .Include(x => x.Person).ThenInclude(x => x.Address).ThenInclude(x => x.City)
               .Include(x => x.JobType)
               .AsNoTracking();

                var serviceDiscounts = _context.ServiceDiscounts
             .Where(bs => bs.DiscountId == DiscountId)
             .Select(bs => bs.Stylist)
             .Include(x => x.Person).ThenInclude(x => x.Address).ThenInclude(x => x.City)
             .Include(x => x.JobType)
             .AsNoTracking();

                var customerDiscounts = _context.CustomerDiscounts
               .Where(bs => bs.DiscountId == DiscountId)
               .Select(bs => bs.Stylist)
               .Include(x => x.Person).ThenInclude(x => x.Address).ThenInclude(x => x.City)
               .Include(x => x.JobType)
               .AsNoTracking();

                var query = discountAssignments.Union(serviceDiscounts).Union(customerDiscounts).Where(x =>
                    (!string.IsNullOrEmpty(x.Person.FirstName) && x.Person.FirstName.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.LastName) && x.Person.LastName.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.NaCode) && x.Person.NaCode.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.PhoneNumber) && x.Person.PhoneNumber.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Email) && x.Person.Email.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Description) && x.Person.Description.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Address.City.CityName.ToString()) && x.Person.Address.City.CityName.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Address.AddressLocationHorizentalPoint.ToString()) && x.Person.Address.AddressLocationHorizentalPoint.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Address.AddressLocationVerticalPoint.ToString()) && x.Person.Address.AddressLocationVerticalPoint.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Address.AddressPostalCode.ToString()) && x.Person.Address.AddressPostalCode.ToString().Contains(searchText)) ||
                    //(!string.IsNullOrEmpty(x.State.ToString()) && x.State.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Address.Description.ToString()) && x.Person.Address.Description.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.Address.AddressStreet.ToString()) && x.Person.Address.AddressStreet.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Person.DateOfBirth.ToString()) && x.Person.DateOfBirth.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.JobType.JobTitle) && x.JobType.JobTitle.Contains(searchText)) ||
                    (x.CreateDate.HasValue && x.CreateDate.Value.ToString().Contains(searchText)) ||
                    (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.YearsOfExperience.ToString()) && x.YearsOfExperience.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Specialty) && x.Specialty.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText))
                );

                results.TotalCount = query.Count();
                results.PageCount = DbTools.GetPageCount(results.TotalCount, pageSize);
                results.Results = await query.OrderByDescending(x => x.CreateDate)
                .SortBy(sortQuery).ToPaging(pageIndex, pageSize)
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

                     // محاسباتی
                     AvgScoreForStylist = _context.RateHistories
                         .Where(x => x.StylistID == r.ID)
                         .Any()
                             ? _context.RateHistories.Where(x => x.StylistID == r.ID).Average(r => r.RateScore)
                             : 0
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
               .Include(x => x.JobType)
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

                        // محاسباتی
                              AvgScoreForStylist = _context.RateHistories
                         .Where(x => x.StylistID == r.ID)
                         .Any()
                             ? _context.RateHistories.Where(x => x.StylistID == r.ID).Average(r => r.RateScore)
                             : 0
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
