using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.AcademicYear
{
    public class GetAcadyearByRangeRequest : DateTimeRange
    {
        public string IdSchool { get; set; }
    }
}