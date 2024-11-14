using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domains;

namespace NobatPlusDATA.Domain
{
    public class ApiGuide : BaseEntity
    {
        public string ApiName { get; set; }
        public string GuideType { get; set; }
        public string ModelName { get; set; }
        public string FieldEnglishName { get; set; }
        public string FieldDataType { get; set; }
        public string FieldFarsiName { get; set; } = "";
        public string FieldRecomendedInputType { get; set; } = "";
    }
}