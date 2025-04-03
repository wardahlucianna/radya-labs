using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.ReportStudentToGc
{
    public class GetDetailReportStudentToGcResult
    {
        public string Id { get; set; }
        public CodeWithIdVm AcademicYear { get; set; }
        public CodeWithIdVm Level { get; set; }
        public List<CodeWithIdVm> Grades { get; set; }
        public DateTime Date { get; set; }
        public List<GetDetailReportStudent> Students { get; set; }
    }

    public class GetDetailReportStudent
    {
        public string Id { get; set; }
        public string IdGcReportStudent { get; set; }
        public string FullName { get; set; }
        public string BinusianId { get; set; }
        public string Level { get; set; }
        public string IdGrade { get; set; }
        public string Grade { get; set; }
        public string Homeroom { get; set; }
        public string Note { get; set; }
    }
}
