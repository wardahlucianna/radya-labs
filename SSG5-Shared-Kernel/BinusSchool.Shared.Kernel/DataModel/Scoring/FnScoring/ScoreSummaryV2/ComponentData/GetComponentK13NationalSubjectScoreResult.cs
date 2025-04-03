using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryV2.ComponentData
{
    public class GetComponentK13NationalSubjectScoreResult
    {
        public GetComponentK13NationalSubjectScoreResult_Header Header { get; set; }
        public List<GetComponentK13NationalSubjectScoreResult_Body> Body { get; set; }
    }
    
    public class GetComponentK13NationalSubjectScoreResult_Header
    {
        public List<GetComponentK13NationalSubjectScoreResult_Header_Component> Component { get; set; }
    }

    public class GetComponentK13NationalSubjectScoreResult_Header_Component
    {
        public string Name { get; set; }
    }

    public class GetComponentK13NationalSubjectScoreResult_Body
    {
        public string Subject { get; set; }
        public string Teacher { get; set; }
        public List<GetComponentK13NationalSubjectScoreResult_Body_ComponentScore> ComponentScore { get; set; }
        public string TotalScore { get; set; }
        public string TotalGrade { get; set; }

    }

    public class GetComponentK13NationalSubjectScoreResult_Body_ComponentScore
    {
        public string ComponentName { get; set; }
        public string Score { get; set; }
        public string Grade { get; set; }
        public string Desc { get; set; }
    }

    public class GetComponentK13NationalSubjectScoreResult_FinalDataVm
    {
        public string IdHomeroom { get; set; }
        public string IdStudent { get; set; }
        public string IdGrade { get; set; }
        public string IdLesson { get; set; }
        public string IdSubject { get; set; }
        public string SubjectDescription { get; set; }
        public string IdSubjectType { get; set; }
        public string IdSubjectLevel { get; set; }
        public string IdSubjectScoreFinalSetting { get; set; }
        public string IdSubjectScoreSemesterSetting { get; set; }
        public string IdSubjectScoreSetting { get; set; }
        public string IdComponent { get; set; }
        public string ComponentName { get; set; }
        public decimal? SemesterScore { get; set; }
        public string? SemesterGrade { get; set; }
        public decimal? ComponentScore { get; set; }
        public string? ComponentGrade { get; set; }
        public int Semester { get; set; }
        public int? SectionSubjectOrderNumber { get; set; }
        public int? ComponentOrderNumber { get; set; }
        public string? IdTeacher { get; set; }
        public string? TeacherName { get; set; }
        public string PeriodDescription { get; set; }

    }
}
