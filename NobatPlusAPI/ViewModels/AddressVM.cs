
using Domains;

namespace AITechWebAPI.ViewModels
{
    public class AddressVM : BaseEntity
    {
        public string AddressStreet { get; set; }
        public string AddressPostalCode { get; set; }
        public string? AddressLocationHorizentalPoint { get; set; }
        public string? AddressLocationVerticalPoint { get; set; }

        public string CityName { get; set; }
        public long CityID { get; set; }
    }
}