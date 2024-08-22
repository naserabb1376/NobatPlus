using Domain;
using NobatPlusAPI.RequestObjects.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.RequestObjects
{
    public class GetBookingListRequestBody:GetListRequestBody
    {
        public long ServiceId { get; set; } = 0;
        public int CancelState { get; set; } = 0;
    }

}
