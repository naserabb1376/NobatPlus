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
        public List<Address> GetAllAddresses(int pageIndex = 1,int pageSize = 20, string searchText ="");
        public Address GetAddressById(long addressId);
        public Address GetAddressByPersonId(long personId);
        public void AddAddress(Address address);
        public void EditAddress(Address address);
        public void RemoveAddress(Address address);
        public void RemoveAddress(long addressId);
        public bool ExistAddress(long addressId);
    }
}
