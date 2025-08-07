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
    public class SocialNetworkRep : ISocialNetworkRep
    {

        private NobatPlusContext _context;
        public SocialNetworkRep()
        {
            _context = DbTools.GetDbContext();
        }

        public async Task<BitResultObject> AddSocialNetworkAsync(SocialNetwork SocialNetwork)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                await _context.SocialNetworks.AddAsync(SocialNetwork);
                await _context.SaveChangesAsync();
                result.ID = SocialNetwork.ID;
                _context.Entry(SocialNetwork).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
           
        }

        public async Task<BitResultObject> EditSocialNetworkAsync(SocialNetwork SocialNetwork)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.SocialNetworks.Update(SocialNetwork);
                await _context.SaveChangesAsync();
                result.ID = SocialNetwork.ID;
                _context.Entry(SocialNetwork).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
           
        }

        public async Task<BitResultObject> ExistSocialNetworkAsync(long SocialNetworkId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                result.Status = await _context.SocialNetworks.AsNoTracking().AnyAsync(x => x.ID == SocialNetworkId);
                result.ID = SocialNetworkId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
             
        }

        public async Task<BitResultObject> ExistSocialNetworkAccountAsync(string socialnetworkName, long stylistId = 0, string phoneNumber = "", string accountLink = "")
        {
            BitResultObject result = new BitResultObject();
            SocialNetwork socialNetwork = new SocialNetwork();

            try
            {
                if (stylistId > 0)
                {
                    if (!string.IsNullOrEmpty(phoneNumber))
                    {
                        socialNetwork = await _context.SocialNetworks.FirstOrDefaultAsync(x => x.StylistID != stylistId && x.SocialNetworkName == socialnetworkName && x.PhoneNumber == phoneNumber );
                    }
                    else if(!string.IsNullOrEmpty(accountLink))
                    {
                        socialNetwork = await _context.SocialNetworks.FirstOrDefaultAsync(x => x.StylistID != stylistId && x.SocialNetworkName == socialnetworkName && x.AccountLink == accountLink);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(phoneNumber))
                    {
                        socialNetwork = await _context.SocialNetworks.FirstOrDefaultAsync(x => x.SocialNetworkName == socialnetworkName && x.PhoneNumber == phoneNumber);
                    }
                    else if (!string.IsNullOrEmpty(accountLink))
                    {
                        socialNetwork = await _context.SocialNetworks.FirstOrDefaultAsync(x => x.SocialNetworkName == socialnetworkName && x.AccountLink == accountLink);
                    }
                }

                result.Status = socialNetwork?.ID > 0;
                result.ID = (socialNetwork?? new SocialNetwork()).ID;

                if (result.Status)
                {
                    result.ErrorMessage = $"این آدرس شبکه اجتماعی برای کاربر دیگری ثبت شده است";
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;

        }

        public async Task<ListResultObject<SocialNetwork>> GetAllSocialNetworksAsync(long stylistId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="")
        {
            ListResultObject<SocialNetwork> results = new ListResultObject<SocialNetwork>();
            try
            {
                IQueryable<SocialNetwork> query;

                query = _context.SocialNetworks
                        .AsNoTracking()
                        .Include(x => x.Stylist).ThenInclude(x => x.Person)
                        .Where(x =>
                            (!string.IsNullOrEmpty(x.Stylist.Person.FirstName.ToString()) && x.Stylist.Person.FirstName.ToString().Contains(searchText))
                            || (!string.IsNullOrEmpty(x.Stylist.Person.LastName.ToString()) && x.Stylist.Person.LastName.ToString().Contains(searchText))
                            || (!string.IsNullOrEmpty(x.AccountLink.ToString()) && x.AccountLink.ToString().Contains(searchText))
                            || (!string.IsNullOrEmpty(x.SocialNetworkName.ToString()) && x.SocialNetworkName.ToString().Contains(searchText))
                            || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
                            || (!string.IsNullOrEmpty(x.PhoneNumber.ToString()) && x.PhoneNumber.ToString().Contains(searchText))
                            || (x.CreateDate.HasValue && x.CreateDate.Value.ToString().Contains(searchText))
                            || (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString().Contains(searchText))
                        );
                if (stylistId > 0)
                {
                    query = query.Where(x=> x.StylistID == stylistId);
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

        public async Task<RowResultObject<SocialNetwork>> GetSocialNetworkByIdAsync(long SocialNetworkId)
        {
            RowResultObject<SocialNetwork> result = new RowResultObject<SocialNetwork>();
            try
            {
                result.Result = await _context.SocialNetworks
                .AsNoTracking()
                .Include(x => x.Stylist).ThenInclude(x => x.Person)
                .SingleOrDefaultAsync(x => x.ID == SocialNetworkId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> RemoveSocialNetworkAsync(SocialNetwork SocialNetwork)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.SocialNetworks.Remove(SocialNetwork);
                await _context.SaveChangesAsync();
                result.ID = SocialNetwork.ID;
                _context.Entry(SocialNetwork).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
           
        }

        public async Task<BitResultObject> RemoveSocialNetworkAsync(long SocialNetworkId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var SocialNetwork = await GetSocialNetworkByIdAsync(SocialNetworkId);
                result = await RemoveSocialNetworkAsync(SocialNetwork.Result);
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
