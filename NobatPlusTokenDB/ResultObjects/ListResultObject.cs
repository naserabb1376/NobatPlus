using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResultObjects
{
    public class ListResultObject<T>
    {
        public bool Status { get; set; } = true;
        public string ErrorMessage { get; set; } = "";
        public int TotalCount { get; set; } = 0;
        public int PageCount { get; set; } = 0;
        public List<T> Results { get; set; } = new List<T>();
    }
}