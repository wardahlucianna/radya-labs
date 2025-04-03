using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryV2.ComponentData
{
    public class GetComponentK13NationalSubjectScoreRequest
    {
        public List<GetSectionSubjectTakenByStudentResult> SetSectionSubjectTakenByStudentList { get; set; }
        public string IdScoreSummaryTabSection { get; set; }
        public string IdStudent { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdGrade { set; get; }
        public int Semester { set; get; }
        public string IdScoreSummaryTab { set; get; }
        public GetScoreSummaryTabSettingResult_BlockPeriod SetBlockPeriod { get; set; }
    }
}
