using Domain;
using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
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

        public async Task AddNotificationAsync(Notification Notification)
        {
            _context.Notifications.Add(Notification);
            await _context.SaveChangesAsync();
            _context.Entry(Notification).State = EntityState.Detached;
        }

        public async Task EditNotificationAsync(Notification Notification)
        {
            _context.Notifications.Update(Notification);
            await _context.SaveChangesAsync();
            _context.Entry(Notification).State = EntityState.Detached;
        }

        public async Task<bool> ExistNotificationAsync(long NotificationId)
        {
            return await _context.Notifications
                .AsNoTracking()
                .AnyAsync(x => x.ID == NotificationId);
        }

        public async Task<List<Notification>> GetAllNotificationsAsync(long personId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            if (personId == 0)
            {
                return await _context.Notifications
                    .AsNoTracking()
                    .Include(x => x.Person)
                    .Where(x =>
                        (!string.IsNullOrEmpty(x.Person.FirstName) && x.Person.FirstName.Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Person.LastName) && x.Person.LastName.Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Person.NaCode) && x.Person.NaCode.Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Person.PhoneNumber) && x.Person.PhoneNumber.Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Person.Email) && x.Person.Email.Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Person.Description) && x.Person.Description.Contains(searchText)) ||
                        (x.Person.DateOfBirth.HasValue && x.Person.DateOfBirth.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Message) && x.Message.Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.SentDate.ToString()) && x.SentDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                        (x.CreateDate.HasValue && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                        (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                    )
                    .OrderByDescending(x => x.CreateDate)
                    .ToPaging(pageIndex, pageSize)
                    .ToListAsync();
            }
            else
            {
                return await _context.Notifications
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
                            (x.Person.DateOfBirth.HasValue && x.Person.DateOfBirth.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Message) && x.Message.Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.SentDate.ToString()) && x.SentDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                            (x.CreateDate.HasValue && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                            (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                        )
                    )
                    .OrderByDescending(x => x.CreateDate)
                    .ToPaging(pageIndex, pageSize)
                    .ToListAsync();
            }
        }

        public async Task<Notification> GetNotificationByIdAsync(long NotificationId)
        {
            return await _context.Notifications
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.ID == NotificationId);
        }

        public async Task RemoveNotificationAsync(Notification Notification)
        {
            _context.Notifications.Remove(Notification);
            await _context.SaveChangesAsync();
            _context.Entry(Notification).State = EntityState.Detached;
        }

        public async Task RemoveNotificationAsync(long NotificationId)
        {
            var Notification = await GetNotificationByIdAsync(NotificationId);
            await RemoveNotificationAsync(Notification);
        }
    }
}
