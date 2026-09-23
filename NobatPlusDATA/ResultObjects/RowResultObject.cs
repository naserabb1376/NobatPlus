using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.ResultObjects
{
    public class RowResultObject<T> : ResultObjectBase
    {
        public string ErrorMessage { get; set; } = "";
        public T Result { get; set; }
    }
}
