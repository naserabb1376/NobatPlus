using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface INotificationRep
    {
        public Task<ListResultObject<Notification>> GetAllNotificationsAsync(long personId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="");
        public Task<RowResultObject<Notification>> GetNotificationByIdAsync(long NotificationId);
        public Task<BitResultObject> AddNotificationAsync(Notification Notification);
        public Task<BitResultObject> EditNotificationAsync(Notification Notification);
        public Task<BitResultObject> RemoveNotificationAsync(Notification Notification);
        public Task<BitResultObject> RemoveNotificationAsync(long NotificationId);
        public Task<BitResultObject> ExistNotificationAsync(long NotificationId);
    }
}
