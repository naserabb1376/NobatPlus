using Domain;
using NobatPlusAPI.RequestObjects.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.RequestObjects
{
    public class ExistLoginRequestBody
    {
        public string UniqueProperty { get; set; }
        public int SearchMode { get; set; }

    }
}
