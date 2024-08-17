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
        public Task<List<Notification>> GetAllNotificationsAsync(long personId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<Notification> GetNotificationByIdAsync(long NotificationId);
        public Task AddNotificationAsync(Notification Notification);
        public Task EditNotificationAsync(Notification Notification);
        public Task RemoveNotificationAsync(Notification Notification);
        public Task RemoveNotificationAsync(long NotificationId);
        public Task<bool> ExistNotificationAsync(long NotificationId);
    }
}
