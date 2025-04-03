using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.CounselorData
{
    public class GetCounselorDataResult : ItemValueVm
    {
        public string AcademicYearCode { get; set; }
        public string Grades { get; set; }
        public string GCPhoto { get; set; }
        public string CounselorName { get; set; }
        public string OfficerLocation { get; set; }
        public string ExtensionNumber { get; set; }
        public string CounselorEmail { get; set; }
        public string OtherInformation { get; set; }
        public AcademicYearObject AcademicYear { get; set; }
    }

    public class AcademicYearObject
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
    }
}
