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
    public class NotificationRep : INotificationRep
    {

        private NobatPlusContext _context;
        public NotificationRep()
        {
            _context = DbTools.GetDbContext();
        }

        public async Task<BitResultObject> AddNotificationAsync(Notification Notification)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                await _context.Notifications.AddAsync(Notification);
                await _context.SaveChangesAsync();
                result.ID = Notification.ID;
                _context.Entry(Notification).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
           
        }

        public async Task<BitResultObject> EditNotificationAsync(Notification Notification)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.Notifications.Update(Notification);
                await _context.SaveChangesAsync();
                result.ID = Notification.ID;
                _context.Entry(Notification).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
           
        }

        public async Task<BitResultObject> ExistNotificationAsync(long NotificationId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                result.Status = await _context.Notifications
                .AsNoTracking()
                .AnyAsync(x => x.ID == NotificationId);
                result.ID = NotificationId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<ListResultObject<Notification>> GetAllNotificationsAsync(long personId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="")
        {
            ListResultObject<Notification> results = new ListResultObject<Notification>();
            try
            {
                IQueryable<Notification> query;

                if (personId == 0)
                {
                    query = _context.Notifications
                        .AsNoTracking()
                        .Include(x => x.Person)
                        .Where(x =>
                            (!string.IsNullOrEmpty(x.Person.FirstName) && x.Person.FirstName.Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Person.LastName) && x.Person.LastName.Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Person.NaCode) && x.Person.NaCode.Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Person.PhoneNumber) && x.Person.PhoneNumber.Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Person.Email) && x.Person.Email.Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Person.Description) && x.Person.Description.Contains(searchText)) ||
                            (x.Person.DateOfBirth != null && x.Person.DateOfBirth.ToString().Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Message) && x.Message.Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.SentDate.ToString()) && x.SentDate.ToString().Contains(searchText)) ||
                            (x.CreateDate.HasValue && x.CreateDate.Value.ToString().Contains(searchText)) ||
                            (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString().Contains(searchText))
                        );
                }
                else
                {
                    query = _context.Notifications
                        .AsNoTracking()
                        .Include(x => x.Person)
                        .Where(x =>
                            x.PersonID == personId &&
                            (
                                (!string.IsNullOrEmpty(x.Person.FirstName) && x.Person.FirstName.Contains(searchText)) ||
                                (!string.IsNullOrEmpty(x.Person.LastName) && x.Person.LastName.Contains(searchText)) ||
                                (!string.IsNullOrEmpty(x.Person.NaCode) && x.Person.NaCode.Contains(searchText)) ||
                                (!string.IsNullOrEmpty(x.Person.PhoneNumber) && x.Person.PhoneNumber.Contains(searchText)) ||
                                (!string.IsNullOrEmpty(x.Person.Email) && x.Person.Email.Contains(searchText)) ||
                                (!string.IsNullOrEmpty(x.Person.Description) && x.Person.Description.Contains(searchText)) ||
                                (x.Person.DateOfBirth != null && x.Person.DateOfBirth.ToString().Contains(searchText)) ||
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

        public async Task<RowResultObject<Notification>> GetNotificationByIdAsync(long NotificationId)
        {
            RowResultObject<Notification> result = new RowResultObject<Notification>();
            try
            {
                result.Result = await _context.Notifications
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.ID == NotificationId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<BitResultObject> RemoveNotificationAsync(Notification Notification)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.Notifications.Remove(Notification);
                await _context.SaveChangesAsync();
                result.ID = Notification.ID;
                _context.Entry(Notification).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
           
        }

        public async Task<BitResultObject> RemoveNotificationAsync(long NotificationId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var Notification = await GetNotificationByIdAsync(NotificationId);
                result = await RemoveNotificationAsync(Notification.Result);
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
