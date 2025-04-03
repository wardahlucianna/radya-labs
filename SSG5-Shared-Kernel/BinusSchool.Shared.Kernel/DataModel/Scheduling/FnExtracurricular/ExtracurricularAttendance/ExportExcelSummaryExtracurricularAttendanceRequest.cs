using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularAttendance
{
    public class ExportExcelSummaryExtracurricularAttendanceRequest
    {
        public string IdSchool { get; set; }
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public Month Month { get; set; }
        public List<string> IdExtracurricular { get; set; }
    }
}
