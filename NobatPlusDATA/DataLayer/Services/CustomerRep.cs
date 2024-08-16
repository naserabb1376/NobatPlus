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
    public class CustomerRep : ICustomerRep
    {

        private NobatPlusContext _context;
        public CustomerRep()
        {
            _context = DbTools.GetDbContext();
        }

        public void AddCustomer(Customer Customer)
        {
            _context.Customers.Add(Customer);
            _context.SaveChanges();
            _context.Entry(Customer).State = EntityState.Detached;
        }

        public void EditCustomer(Customer Customer)
        {
            _context.Customers.Update(Customer);
            _context.SaveChanges();
            _context.Entry(Customer).State = EntityState.Detached;
        }

        public bool ExistCustomer(long CustomerId)
        {
            return _context.Customers.Any(x => x.ID == CustomerId);
        }

        public List<Customer> GetAllCustomers(int pageIndex= 1, int pageSize = 20, string searchText= "")
        {
            return _context.Customers.Include(x => x.Person).Where(x =>
             (!string.IsNullOrEmpty(x.Person.FirstName.ToString()) && x.Person.FirstName.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Person.LastName.ToString()) && x.Person.LastName.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Person.NaCode.ToString()) && x.Person.NaCode.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Person.PhoneNumber.ToString()) && x.Person.PhoneNumber.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Person.Email.ToString()) && x.Person.Email.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Person.Description.ToString()) && x.Person.Description.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Person.DateOfBirth.ToString()) && x.Person.DateOfBirth.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
            || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
            || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
             ).OrderByDescending(x=> x.CreateDate).ToPaging(pageIndex, pageSize).ToList();
        }

        public List<Customer> GetCustomersOfDiscount(long DiscountId, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            return _context.CustomerDiscounts
            .Where(bs => bs.DiscountId == DiscountId)
            .Select(bs => bs.Customer).Include(x => x.Person).Where(x =>
            (!string.IsNullOrEmpty(x.Person.FirstName.ToString()) && x.Person.FirstName.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Person.LastName.ToString()) && x.Person.LastName.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Person.NaCode.ToString()) && x.Person.NaCode.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Person.PhoneNumber.ToString()) && x.Person.PhoneNumber.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Person.Email.ToString()) && x.Person.Email.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Person.Description.ToString()) && x.Person.Description.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Person.DateOfBirth.ToString()) && x.Person.DateOfBirth.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
           || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
           || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
            ).OrderByDescending(x => x.CreateDate).ToPaging(pageIndex, pageSize).ToList();
        }

        public Customer GetCustomerById(long CustomerId)
        {
            return _context.Customers.Find(CustomerId);
        }

        public void RemoveCustomer(Customer Customer)
        {
            _context.Customers.Remove(Customer);
            _context.SaveChanges();
            _context.Entry(Customer).State = EntityState.Detached;
        }

        public void RemoveCustomer(long CustomerId)
        {
            var Customer = GetCustomerById(CustomerId);
            RemoveCustomer(Customer);
        }
    }
}
