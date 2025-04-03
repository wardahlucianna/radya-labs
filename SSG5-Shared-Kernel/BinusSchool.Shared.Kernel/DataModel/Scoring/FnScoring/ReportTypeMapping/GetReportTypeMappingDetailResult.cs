using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportTypeMapping
{
    public class GetReportTypeMappingDetailResult
    {
        public ItemValueVm AcademicYear { set; get; }
        public ItemValueVm ReportType { set; get; }
        public ItemValueVm ReportTemplate { set; get; }
        public List<GetReportTypeMappingDetail_LevelVm> Levels { set; get; }
        public bool CurrentStatus { set; get; }
    }

    public class GetReportTypeMappingDetail_LevelVm
    {
        public string Code { set; get; }
        public string IdLevel { set; get; }
        public string LevelName { set; get; }
        public List<GetReportTypeMappingDetail_GradeVm> Grades { set; get; }
    }

    public class GetReportTypeMappingDetail_GradeVm
    {
        public string IdGrade { set; get; }
        public string GradeName { set; get; }
        public bool IsUsedReportTemplate { set; get; }
        public bool isUsedReportType { set; get; }
    }
}
