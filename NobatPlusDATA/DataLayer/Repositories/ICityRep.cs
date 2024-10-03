using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface ICityRep
    {
        public Task<ListResultObject<City>> GetAllCitiesAsync(long parentId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="");
        public Task<RowResultObject<City>> GetCityByIdAsync(long CityId);
        public Task<BitResultObject> AddCityAsync(City City);
        public Task<BitResultObject> EditCityAsync(City City);
        public Task<BitResultObject> RemoveCityAsync(City City);
        public Task<BitResultObject> RemoveCityAsync(long CityId);
        public Task<BitResultObject> ExistCityAsync(long CityId);
    }
}
