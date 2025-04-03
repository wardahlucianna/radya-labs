using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.School;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportData.MasterTemplateData
{
    public class CreateHeaderBNSReportRequest
    {
        public string ContainerName { get; set; }
        public string IdReportTemplate { get; set; }
        public string IdReportType { get; set; }
        public string TemplateName { get; set; }
        public GetSchoolDetailResult School { get; set; }
        public CodeWithIdVm AcademicYear { get; set; }
        public NameValueVm Student { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public string IdPeriod { get; set; }
        public CodeWithIdVm Homeroom { get; set; }
        public NameValueVm CA { get; set; }
        public NameValueVm Principal { get; set; }
        public StudentDataBNSVm StudentData { get; set; }
        public int Semester { get; set; }
    }

    public class StudentDataBNSVm 
    {
        public string POB { get; set; }
        public string DOB { get; set; }
        public string Gender { get; set; }
    }

}
