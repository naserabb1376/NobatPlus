using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domains;

namespace NobatPlusDATA.Domain
{
    public class RateHistory : BaseEntity
    {
        public long WorkerID { get; set; }
        public long CustomerID { get; set; }
        public long BookingID { get; set; }
        public long RateQuestionID { get; set; }
        public float RateScore { get; set; }
      
        public DateTime RateDate { get; set; }

        public Worker Worker { get; set; }
        public Customer Customer { get; set; }
        public Booking Booking { get; set; }
        public RateQuestion RateQuestion { get; set; }

    }

    public class RateHistoryDTO: BaseEntity
    {
        public long WorkerID { get; set; }
        public long CustomerID { get; set; }
        public long BookingID { get; set; }
        public long RateQuestionID { get; set; }
        public float RateScore { get; set; }
        public DateTime RateDate { get; set; }

        // Navigation Properties
        public Worker Worker { get; set; }
        public Customer Customer { get; set; }
        public Booking Booking { get; set; }
        public RateQuestion RateQuestion { get; set; }

        // محاسباتی
        public float AvgScorePerQuestion { get; set; }
        public float AvgScoreForWorker { get; set; }
    }

}