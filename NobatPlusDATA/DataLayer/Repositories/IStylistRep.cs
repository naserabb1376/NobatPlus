using NobatPlusDATA.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IStylistRep
    {
        public List<Stylist> GetAllStylists(long parentId = 0,int pageIndex = 1,int pageSize = 20, string searchText ="");
        public List<Stylist> GetStylistsOfDiscount(long DiscountId,int pageIndex = 1,int pageSize = 20, string searchText ="");
        public List<Stylist> GetStylistsOfService(long serviceManagementId,int pageIndex = 1,int pageSize = 20, string searchText ="");
        public List<Stylist> GetStylistsOfJobType(long JobTypeId,int pageIndex = 1,int pageSize = 20, string searchText ="");
        public Stylist GetStylistById(long StylistId);
        public void AddStylist(Stylist Stylist);
        public void EditStylist(Stylist Stylist);
        public void RemoveStylist(Stylist Stylist);
        public void RemoveStylist(long StylistId);
        public bool ExistStylist(long StylistId);
    }
}
