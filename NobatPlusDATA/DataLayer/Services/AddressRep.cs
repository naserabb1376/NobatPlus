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

        public async Task AddAddressAsync(Address address)
        {
            await _context.Addresses.AddAsync(address);
            await _context.SaveChangesAsync();
            _context.Entry(address).State = EntityState.Detached;
        }

        public async Task EditAddressAsync(Address address)
        {
            _context.Addresses.Update(address);
            await _context.SaveChangesAsync();
            _context.Entry(address).State = EntityState.Detached;
        }

        public async Task<bool> ExistAddressAsync(long addressId)
        {
            return await _context.Addresses.AsNoTracking().AnyAsync(x => x.ID == addressId);
        }

        public async Task<List<Address>> GetAllAddressesAsync(int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            return await _context.Addresses
                .AsNoTracking()
                .Where(x =>
                    (!string.IsNullOrEmpty(x.AddressCity.ToString()) && x.AddressCity.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.AddressLocationHorizentalPoint.ToString()) && x.AddressLocationHorizentalPoint.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.AddressLocationVerticalPoint.ToString()) && x.AddressLocationVerticalPoint.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.AddressPostalCode.ToString()) && x.AddressPostalCode.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.State.ToString()) && x.State.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.AddressStreet.ToString()) && x.AddressStreet.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                )
                .OrderByDescending(x => x.CreateDate)
                .ToPaging(pageIndex, pageSize)
                .ToListAsync();
        }

        public async Task<Address> GetAddressByIdAsync(long addressId)
        {
            return await _context.Addresses.AsNoTracking().SingleOrDefaultAsync(x => x.ID == addressId);
        }

        public async Task<Address> GetAddressByPersonIdAsync(long personId)
        {
            var person = await _context.Persons
                .Include(x => x.Address)
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.ID == personId);
            return person?.Address;
        }

        public async Task RemoveAddressAsync(Address address)
        {
            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();
            _context.Entry(address).State = EntityState.Detached;
        }

        public async Task RemoveAddressAsync(long addressId)
        {
            var address = await GetAddressByIdAsync(addressId);
            await RemoveAddressAsync(address);
        }

    }
}
