using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.Views
{
    public class V_Customer
    {
        public string FirstName { get; set; }
        public int ID { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string PersonDescription { get; set; }
        public string AddressLocationVerticalPoint { get; set; }
        public string AddressLocationHorizentalPoint { get; set; }
        public string AddressPostalCode { get; set; }
        public string AddressStreet { get; set; }
        public string CityName { get; set; }
        public int? CityParentID { get; set; }
        public int CityID { get; set; }
        public int AddressID { get; set; }
        public int CustomerID { get; set; }
        public string CustomerDescription { get; set; }
        public string AddressDescription { get; set; }
        public string CityDescription { get; set; }
        public string NaCode { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
    }
}