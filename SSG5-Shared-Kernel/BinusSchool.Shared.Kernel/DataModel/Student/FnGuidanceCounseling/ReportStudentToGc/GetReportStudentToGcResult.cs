using System;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.ReportStudentToGc
{
    public class GetReportStudentToGcResult : CodeWithIdVm
    {
        public string Id { get; set; }
        public CodeWithIdVm Level { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public CodeWithIdVm ClassHomeroom { get; set; }
        public CodeWithIdVm StudentPhoto { get; set; }
        public string AcademicYear { get; set; }
        public string StudentName { get; set; }
        public string BinusanId { get; set; }
        public DateTime ReportStudentDate { get; set; }
        public string ReportStudentNote { get; set; }
        public bool IsRead { get; set; }
    }

    public class CodeWithIdVmOrder : CodeWithIdVm
    {
        public int OrderNumber {  get; set; }
    }
}
