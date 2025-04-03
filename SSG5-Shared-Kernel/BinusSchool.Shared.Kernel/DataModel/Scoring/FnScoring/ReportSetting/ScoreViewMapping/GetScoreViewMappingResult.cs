using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportSetting.ScoreViewMapping
{
    public class GetScoreViewMappingResult : CodeWithIdVm
    {
        public string IdAcademicYear { set; get; }
        public string AcademicYearName { set; get; }
        public string IdLevel { set; get; }
        public string LevelName { set; get; }
        public int LevelOrder { set; get; }
        public string IdGrade { set; get; }
        public string GradeName { set; get; }
        public int GradeOrder { set; get; }
        public string IdReportScoreViewTemplate { set; get; }
        public string ScoreViewTemplateDesc { set; get; }
        public string CodeScoreView { set; get; }
        public string IdSubject { set; get; }
        public string SubjectName { set; get; }
        public bool CurrentStatus { set; get; }
    }
}
