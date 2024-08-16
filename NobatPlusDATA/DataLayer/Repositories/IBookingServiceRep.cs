using NobatPlusDATA.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IBookingServiceRep
    {
        public List<BookingService> GetAllBookingServices(int pageIndex = 1,int pageSize = 20, string searchText ="");
        public BookingService GetBookingServiceById(long BookingId, long ServiceManagementId);
        public void AddBookingService(BookingService BookingService);
        public void EditBookingService(BookingService BookingService);
        public void RemoveBookingService(BookingService BookingService);
        public void RemoveBookingService(long BookingId, long ServiceManagementId);
        public bool ExistBookingService(long BookingId, long ServiceManagementId);
    }
}
