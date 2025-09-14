using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domains;

namespace NobatPlusDATA.ViewModels
{
    public class RateHistoryVM : BaseEntity
    {
        public string StylistName { get; set; }
        public string SalonName { get; set; }
        public string CustomerName { get; set; }
        public string RateQuestionText { get; set; }
        public float RateScore { get; set; }
        public float AvgScorePerQuestion { get; set; }
        public float AvgScoreForStylist { get; set; }
        public DateTime RateDate { get; set; }

    }
}