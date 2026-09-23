using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.ResultObjects
{
    public class ListResultObject<T> : ResultObjectBase
    {
        public string ErrorMessage { get; set; } = "";
        public int TotalCount { get; set; } = 0;
        public int PageCount { get; set; } = 0;
        public List<T> Results { get; set; } = new List<T>();
    }
}
