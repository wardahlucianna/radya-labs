using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.MapStudentHomeroom
{
    public class GetMapStudentHomeroomAvailableRequest : CollectionRequest
    {
        public string IdGrade { get; set; }
        public string IdHomeroom { get; set; }
        public IEnumerable<string> ExceptIds { get; set; }
    }
}