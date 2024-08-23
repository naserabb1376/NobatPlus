using Domain;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IAddressRep
    {
        public Task<ListResultObject<Address>> GetAllAddressesAsync(long CityId = 0,int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<RowResultObject<Address>> GetAddressByIdAsync(long addressId);
        public Task<RowResultObject<Address>> GetAddressByPersonIdAsync(long personId);
        public Task<BitResultObject> AddAddressAsync(Address address);
        public Task<BitResultObject> EditAddressAsync(Address address);
        public Task<BitResultObject> RemoveAddressAsync(Address address);
        public Task<BitResultObject> RemoveAddressAsync(long addressId);
        public Task<BitResultObject> ExistAddressAsync(long addressId);
    }
}
