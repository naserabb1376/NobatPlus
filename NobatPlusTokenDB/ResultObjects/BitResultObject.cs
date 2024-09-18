using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResultObjects
{
    public class BitResultObject
    {
        public bool Status { get; set; } = true;
        public string ErrorMessage { get; set; } = "";
        public long ID { get; set; } = 0;
    }
}