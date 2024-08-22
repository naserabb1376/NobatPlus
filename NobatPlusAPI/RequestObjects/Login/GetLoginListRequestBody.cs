using Domain;
using NobatPlusAPI.RequestObjects.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.RequestObjects
{
    public class GetLoginListRequestBody:GetListRequestBody
    {
        public long PersonId { get; set; } = 0;

    }
}
