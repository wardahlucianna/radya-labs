﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryV2.ComponentData
{
    public class GetComponentGradingScoreGroupRequest
    {
        public List<GetSectionSubjectTakenByStudentResult> SetSectionSubjectTakenByStudentList { get; set; }
        public GetScoreSummaryTabSettingResult_BlockPeriod SetBlockPeriod { get; set; }
        public string IdScoreSummaryTabSection { get; set; }
        public string IdStudent { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdGrade { set; get; }
        public int Semester { set; get; }
        public string IdScoreSummaryTab { set; get; }
        public List<string> IdLesson { get; set; }
        public string IdSubjectLevel { get; set; }
        public string IdSubject { get; set; }
        public string SubjectName { get; set; }
    }
}
