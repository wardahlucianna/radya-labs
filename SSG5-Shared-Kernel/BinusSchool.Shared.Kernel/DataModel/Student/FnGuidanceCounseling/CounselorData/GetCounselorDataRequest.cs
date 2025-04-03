using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.CounselorData
{
    public class GetCounselorDataRequest : CollectionRequest
    {
        public string IdAcadyear { get; set; }
        public string AcademicYearCode { get; set; }
        public string Grades { get; set; }
        public string GCPhoto { get; set; }
        public string ConseloerName { get; set; }
        public string OfficerLocation { get; set; }
        public string ExtensionNumber { get; set; }
        public string ConselorEmail { get; set; }
        public string OtherInformation { get; set; }
    }
}
