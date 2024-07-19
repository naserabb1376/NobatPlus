using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domains;

namespace Domain
{
    internal class Market : BaseEntity
    {
        [Display(Name = "نام")]
        [MinLength(500)]
        public string MarketName { get; set; }

        [Display(Name = "شماره تماس ثابت")]
        [MinLength(500)]
        public String? MarketTelephonNumber { get; set; }

        [Display(Name = "شماره تماس همراه")]
        [MinLength(500)]
        public String? MarketMobileNumber { get; set; }
    }
}