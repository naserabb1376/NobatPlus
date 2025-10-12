using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IStylistPacificRep
    {
        public Task<ListResultObject<StylistPacific>> GetAllStylistPacificsAsync(long stylistId=0,int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="");
        public Task<RowResultObject<StylistPacific>> GetStylistPacificByIdAsync(long StylistPacificId);
        public Task<BitResultObject> AddStylistPacificAsync(StylistPacific StylistPacific);
        public Task<BitResultObject> EditStylistPacificAsync(StylistPacific StylistPacific);
        public Task<BitResultObject> RemoveStylistPacificAsync(StylistPacific StylistPacific);
        public Task<BitResultObject> RemoveStylistPacificAsync(long StylistPacificId);
        public Task<BitResultObject> ExistStylistPacificAsync(long StylistPacificId);
    }
}
