using NobatPlusDATA.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface INotificationRep
    {
        public List<Notification> GetAllNotifications(long personId = 0,int pageIndex = 1,int pageSize = 20, string searchText ="");
        public Notification GetNotificationById(long NotificationId);
        public void AddNotification(Notification Notification);
        public void EditNotification(Notification Notification);
        public void RemoveNotification(Notification Notification);
        public void RemoveNotification(long NotificationId);
        public bool ExistNotification(long NotificationId);
    }
}
