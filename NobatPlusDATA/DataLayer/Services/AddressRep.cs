using Domain;
using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Services
{
    public class AddressRep : IAddressRep
    {

        private NobatPlusContext _context;
        public AddressRep()
        {
            _context = DbTools.GetDbContext();
        }

        public void AddAddress(Address Address)
        {
            _context.Addresses.Add(Address);
            _context.SaveChanges();
            _context.Entry(Address).State = EntityState.Detached;
        }

        public void EditAddress(Address Address)
        {
            _context.Addresses.Update(Address);
            _context.SaveChanges();
            _context.Entry(Address).State = EntityState.Detached;
        }

        public bool ExistAddress(long AddressId)
        {
            return _context.Addresses.Any(x => x.ID == AddressId);
        }

        public List<Address> GetAllAddresses(int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            return _context.Addresses.Where(x =>
            (!string.IsNullOrEmpty(x.AddressCity.ToString()) && x.AddressCity.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.AddressLocationHorizentalPoint.ToString()) && x.AddressLocationHorizentalPoint.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.AddressLocationVerticalPoint.ToString()) && x.AddressLocationVerticalPoint.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.AddressPostalCode.ToString()) && x.AddressPostalCode.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.State.ToString()) && x.State.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.AddressStreet.ToString()) && x.AddressStreet.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
           || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
            ).OrderByDescending(x => x.CreateDate).ToPaging(pageIndex,pageSize).ToList();
        }

        public Address GetAddressById(long AddressId)
        {
            return _context.Addresses.Find(AddressId);
        }

        public Address GetAddressByPersonId(long personId)
        {
            var person = _context.Persons.Include(x=> x.Address).SingleOrDefault(x=> x.ID == personId);
            return person.Address;
        }

        public void RemoveAddress(Address Address)
        {
            _context.Addresses.Remove(Address);
            _context.SaveChanges();
            _context.Entry(Address).State = EntityState.Detached;
        }

        public void RemoveAddress(long AddressId)
        {
            var Address = GetAddressById(AddressId);
            RemoveAddress(Address);
        }

    }
}
