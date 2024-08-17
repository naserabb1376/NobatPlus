using Domain;
using NobatPlusDATA.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IAddressRep
    {
        public Task<List<Address>> GetAllAddressesAsync(int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<Address> GetAddressByIdAsync(long addressId);
        public Task<Address> GetAddressByPersonIdAsync(long personId);
        public Task AddAddressAsync(Address address);
        public Task EditAddressAsync(Address address);
        public Task RemoveAddressAsync(Address address);
        public Task RemoveAddressAsync(long addressId);
        public Task<bool> ExistAddressAsync(long addressId);
    }
}
