using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.ResultObjects
{
    public class BitResultObject : ResultObjectBase
    {
        public string ErrorMessage { get; set; } = "";
        public long ID { get; set; } = 0;
    }
}
