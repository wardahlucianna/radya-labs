using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportTypeMapping
{
    public class GetReportTypeGradeMappedResult
    {
        public string Code { set; get; }
        public string IdLevel { set; get; }
        public string LevelName { set; get; }
        public List<GetReportTypeGradeMapped_GradeVm> Grades { set; get; }
    }

    public class GetReportTypeGradeMapped_GradeVm
    {
        public string IdGrade { set; get; }
        public string GradeName { set; get; }
        public bool IsUsedReportTemplate { set; get; }
        public bool isUsedReportType { set; get; }
    }
}
