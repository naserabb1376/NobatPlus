using Domain;
using NobatPlusAPI.RequestObjects.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.RequestObjects
{
    public class GetJobTypeListRequestBody:GetListRequestBody
    {
        public int SexTypeChecked { get; set; }

    }
}
