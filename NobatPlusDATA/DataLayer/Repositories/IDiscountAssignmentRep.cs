using NobatPlusDATA.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IDiscountAssignmentRep
    {
        public List<DiscountAssignment> GetAllDiscountAssignments(long DiscountId, long AdminId = 0 , long StylistId = 0,int pageIndex = 1,int pageSize = 20, string searchText ="");
        public DiscountAssignment GetDiscountAssignmentById(long DiscountAssignmentId);
        public void AddDiscountAssignment(DiscountAssignment DiscountAssignment);
        public void EditDiscountAssignment(DiscountAssignment DiscountAssignment);
        public void RemoveDiscountAssignment(DiscountAssignment DiscountAssignment);
        public void RemoveDiscountAssignment(long DiscountAssignmentId);
        public bool ExistDiscountAssignment(long DiscountAssignmentId);
    }
}
