using Domain;
using NobatPlusAPI.Models.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.SocialNetwork
{
    public class GetSocialNetworkListRequestBody : GetListRequestBody
    {
        [Display(Name = "کد خدمات دهنده")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long StylistId { get; set; } = 0;  
    }

}
