using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryV2.ComponentData
{
    public class GetComponentSubjectScoreResult
    {
        public GetComponentSubjectScoreResult_Header Header { get; set; }
        public GetComponentSubjectScoreResult_Body Body { get; set; }
    }

    public class GetComponentSubjectScoreResult_Header
    {
        public List<GetComponentSubjectScoreResult_Header_Semester> SemesterList { get; set; }
    }

    public class GetComponentSubjectScoreResult_Header_Semester
    {
        public string IdSemester { get; set; }
        public string SemesterName { get; set; }
        public List<GetComponentSubjectScoreResult_Header_Semester_Term> TermList { get; set; }
    }

    public class GetComponentSubjectScoreResult_Header_Semester_Term
    {
        
        public string IdPeriod { get; set; }
        public string PeriodName { get; set; }
        public List<string> ComponentShortDesc { get; set; }
    }

    public class GetComponentSubjectScoreResult_Body
    {
        public List<GetComponentSubjectScoreResult_Body_SubjectList> SubjectList { get; set; }
        public string OverallSemester1Score { get; set; }
        public string OverallSemester2Score { get; set; }
        public string OverallFinalScore { get; set; }
    }
    public class GetComponentSubjectScoreResult_Body_SubjectList
    {
        public string IdSubject { get; set; }
        public string SubjectName { get; set; }
        public List<GetComponentSubjectScoreResult_Body_SubjectList_SemesterScore> SemesterScoreList { get; set; }
        public string TeacherName { get; set; }
        public string FinalScore { get; set; }
        public string FinalGrade { get; set; }
    }

    public class GetComponentSubjectScoreResult_Body_SubjectList_TermScore
    {
        public string IdComponent { get; set; }
        public string IdPeriod { get; set; }
        public string ComponentShortDesc { get; set; }
        public string? ComponentScore { get; set; }
    }

    public class GetComponentSubjectScoreResult_Body_SubjectList_SemesterScore
    {
        public string IdSemester { get; set; }
        public string SemesterScore { get; set; }
        public List<GetComponentSubjectScoreResult_Body_SubjectList_TermScore> ComponentScoreList { get; set; }
    }

    //public class GetComponentSubjectScoreResult_ComponentSubjectList
    //{
    //    public string IdSubject { get; set; }
    //    public string SubjectName { get; set; }
    //    public string IdLesson { get; set; }
    //    public int Semester { get; set; }
    //    public string? ComponentScore { get; set; }
    //    public string ComponentGrading { get; set; }
    //    public string ScoreSemester { get; set; }
    //    public string FinalScore { get; set; }
    //    public string FinalGrade { get; set; }
    //    public string IdSubjectScoreFinalSetting { get; set; }
    //    public string IdSubjectScoreSemesterSetting { get; set; }
    //    public string IdSubjectScoreSetting { get; set; }
    //    public string IdComponent { get; set; }
    //    public string IdPeriod { get; set;}
    //    public string PeriodDescription { get; set; }
    //    public string PeriodOrderNumber { get; set; }
    //    public string ComponentLongdesc { get; set; }
    //    public string ComponentShortDesc { get; set; }
    //    public string TeacherName { get; set; }
    //    public string IdTeacher { get; set; }
    //    public int ShowGradingSubjectScoreSetting { get; set; }
    //    public int ShowGradingComponent { get; set; }
    //    public string ComponentOrderNumber
    //}
}
