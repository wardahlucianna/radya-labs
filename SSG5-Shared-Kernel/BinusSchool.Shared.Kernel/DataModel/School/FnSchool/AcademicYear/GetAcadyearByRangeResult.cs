using System;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.AcademicYear
{
    public class GetAcadyearByRangeResult : CodeWithIdVm
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}