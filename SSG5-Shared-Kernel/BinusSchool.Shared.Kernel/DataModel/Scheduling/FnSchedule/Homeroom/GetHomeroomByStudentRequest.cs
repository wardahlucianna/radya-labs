using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.Homeroom
{
    public class GetHomeroomByStudentRequest : IdCollection
    {
        public IEnumerable<string> IdGrades { get; set; }
    }
}