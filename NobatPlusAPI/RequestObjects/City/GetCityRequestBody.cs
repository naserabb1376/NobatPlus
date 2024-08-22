using Domain;
using NobatPlusAPI.RequestObjects.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.RequestObjects
{
    public class GetCityListRequestBody:GetListRequestBody
    {
        public long ParentId { get; set; } = -1;

    }
}
