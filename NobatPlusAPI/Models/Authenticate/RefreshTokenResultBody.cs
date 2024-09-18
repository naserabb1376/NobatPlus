using NobatPlusAPI.Tools;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Authenticate
{
    public class RefreshTokenResultBody
    {
        public string RefreshToken { get; set; }
        public string AccessToken { get; set; }
    }
}
