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

        public void AddNotification(Notification Notification)
        {
            _context.Notifications.Add(Notification);
            _context.SaveChanges();
            _context.Entry(Notification).State = EntityState.Detached;
        }

        public void EditNotification(Notification Notification)
        {
            _context.Notifications.Update(Notification);
            _context.SaveChanges();
            _context.Entry(Notification).State = EntityState.Detached;
        }

        public bool ExistNotification(long NotificationId)
        {
            return _context.Notifications.Any(x => x.ID == NotificationId);
        }

        public List<Notification> GetAllNotifications(long personId = 0, int pageIndex= 1, int pageSize = 20, string searchText= "")
        {
            if (personId == 0)
            {
            return _context.Notifications.Include(x => x.Person).Where(x =>
             (!string.IsNullOrEmpty(x.Person.FirstName.ToString()) && x.Person.FirstName.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Person.LastName.ToString()) && x.Person.LastName.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Person.NaCode.ToString()) && x.Person.NaCode.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Person.PhoneNumber.ToString()) && x.Person.PhoneNumber.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Person.Email.ToString()) && x.Person.Email.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Person.Description.ToString()) && x.Person.Description.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Person.DateOfBirth.ToString()) && x.Person.DateOfBirth.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
            || (!string.IsNullOrEmpty(x.Message.ToString()) && x.Message.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.SentDate.ToString()) && x.SentDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
            || (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
            || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
             ).OrderByDescending(x=> x.CreateDate).ToPaging(pageIndex, pageSize).ToList();
            }
            else
            {
                return _context.Notifications.Include(x => x.Person).Where(x =>
                (x.PersonID == personId) &&
             ((!string.IsNullOrEmpty(x.Person.FirstName.ToString()) && x.Person.FirstName.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Person.LastName.ToString()) && x.Person.LastName.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Person.NaCode.ToString()) && x.Person.NaCode.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Person.PhoneNumber.ToString()) && x.Person.PhoneNumber.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Person.Email.ToString()) && x.Person.Email.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Person.Description.ToString()) && x.Person.Description.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Person.DateOfBirth.ToString()) && x.Person.DateOfBirth.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
            || (!string.IsNullOrEmpty(x.Message.ToString()) && x.Message.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.SentDate.ToString()) && x.SentDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
            || (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
            || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
             )).OrderByDescending(x => x.CreateDate).ToPaging(pageIndex, pageSize).ToList();
            }
            
        }

        public Notification GetNotificationById(long NotificationId)
        {
            return _context.Notifications.Find(NotificationId);
        }

        public void RemoveNotification(Notification Notification)
        {
            _context.Notifications.Remove(Notification);
            _context.SaveChanges();
            _context.Entry(Notification).State = EntityState.Detached;
        }

        public void RemoveNotification(long NotificationId)
        {
            var Notification = GetNotificationById(NotificationId);
            RemoveNotification(Notification);
        }
    }
}
