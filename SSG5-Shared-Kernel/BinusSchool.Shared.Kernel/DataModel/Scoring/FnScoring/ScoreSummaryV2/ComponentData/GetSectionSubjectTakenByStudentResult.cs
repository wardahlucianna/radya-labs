using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryV2.ComponentData
{
    public class GetSectionSubjectTakenByStudentResult
    {
        public string ScoreSummaryTabDesc { get; set; }
        public string IdScoreSummaryTabSection { get; set; }
        public string IdLesson { get; set; }
        public string IdSubjectScoreFinalSetting { get; set; }
        public string IdSubject { get; set; }
        public string SubjectName { set; get; }
        public string IdSubjectType { set; get; }
        public string IdSubjectLevel { set; get; }
        public string SubjectID { get; set; }
        public int OrderNumberSectionSubject { get; set; }
        public int Semester { get; set; }
        public bool ShowTeacherName { set; get; }
        public bool ShowTotal { set; get; }
        public bool ShowSubjectLevel { set; get; }
        public bool HideInSemesterOne { set; get; }
    }

    public class GetSectionSubjectTakenByStudentResult_SubjectFinalCombined
    {
        public List<GetSectionSubjectTakenByStudentResult> SubjectFinalCombined { get; set; }
        public List<GetComponentIB2023SubjectScoreResult_FinalCombineScore> SubjectFinalCombinedScore { get; set; }
        public string GradeValue { set; get; }
    }

    public class GetSectionSubjectTakenByStudentResult_ComponentVM_GetAllSubjectPerSection
    {
        public string ScoreSummaryTabDesc { get; set; }
        public string IdScoreSummaryTabSection { get; set; }    
        public string IdSubjectType { get; set; }
        public string SubjectID { get; set; }
        public int OrderNumber { get; set; }
        public bool ShowTeacherName { get; set; }
        public bool ShowTotal { get; set; }
        public bool ShowSubjectLevel { get; set; }
        public bool HideInSemesterOne { get; set; }
    }
}
