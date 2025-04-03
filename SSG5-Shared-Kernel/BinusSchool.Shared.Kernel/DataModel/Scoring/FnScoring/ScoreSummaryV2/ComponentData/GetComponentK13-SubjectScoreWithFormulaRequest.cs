using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryV2.ComponentData
{
    public class GetComponentK13_SubjectScoreWithFormulaRequest
    {
        public GetScoreSummaryTabSettingResult_BlockPeriod SetBlockPeriod { get; set; }
        public List<GetSectionSubjectTakenByStudentResult> SetSectionSubjectTakenByStudentList { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdStudent { get; set; }
        public string IdGrade { get; set; }
        public int Semester { get; set; }
        public string IdScoreSummaryTab { get; set; }
    }
}
