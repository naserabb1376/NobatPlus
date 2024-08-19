using Domain;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.RequestObjects.JobType
{
    public class AddEditJobTypeRequestBody
    {
        public long ID { get; set; }
        public string JobTitle { get; set; }
        public int SexTypeChecked { get; set; }
        public string? Description { get; set; }

    }
}
