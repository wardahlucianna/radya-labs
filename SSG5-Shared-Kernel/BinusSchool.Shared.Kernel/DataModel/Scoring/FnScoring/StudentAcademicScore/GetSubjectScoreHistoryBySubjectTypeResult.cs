using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.StudentAcademicScore
{
    public class GetSubjectScoreHistoryBySubjectTypeResult
    {
        public List<GetSubjectScoreHistoryBySubjectTypeResult_Header> Header { get; set; }
        public List<GetSubjectScoreHistoryBySubjectTypeResult_Body> Body { get; set; }
    }

    public class GetSubjectScoreHistoryBySubjectTypeResult_Header : NameValueVm
    {
        public List<GetSubjectScoreHistoryBySubjectTypeResult_Component> ComponentList { get; set; }
    }
    public class GetSubjectScoreHistoryBySubjectTypeResult_Component
    {
        public string ComponentShortDesc { get; set; }
        public string ComponentLongDesc { get; set; }
    }

    public class GetSubjectScoreHistoryBySubjectTypeResult_Body
    {
        public NameValueVm Grade { get; set; }
        public string Teacher { get; set; }
        public List<GetSubjectScoreHistoryBySubjectTypeResult_Body_SubjectScore> SubjectScore { get; set; }
    }
    public class GetSubjectScoreHistoryBySubjectTypeResult_Body_SubjectScore
    {
        public string SubjectScore { get; set; }
        public string SubjectScoreLongDesc { get; set; }
        public string Score { get; set; }
    }

    public class GetSubjectScoreHistoryBySubjectTypeResult_HomeroomStudent
    {
        public string IdAcademicYear { get; set; }
        public int AcademicYearCodeInt { get; set; }
        public string AcademicYearCode { get; set; }
        public string AcademicYearDesc { get; set; }
        public string IdGrade { get; set; }
        public string GradeDesc { get; set; }
        public string HomeroomDesc { get; set; }
        public int OrderNumbeAcademicYear { get; set; }
        public int OrderNumbeLevel { get; set; }
        public int Semester { get; set; }
    }

    public class GetSubjectScoreHistoryBySubjectTypeResult_StudentComponentScore
    {
        public string IdAcademicYear { set; get; }
        public int AcademicYearCodeInt { get; set; }
        public string AcademicYearCode { get; set; }
        public int OrderNumbeAcademicYear { get; set; }
        public string IdSubject { set; get; }
        public string IdLesson { set; get; }
        public string IdGrade { set; get; }
        public string GradeDesc { set; get; }
        public int Semester { set; get; }
        public string Term { set; get; }
        public string IdComponent { set; get; }
        public string ComponentShortDesc { set; get; }
        public string ComponentLongDesc { set; get; }
        public bool ShowGradingAsScore { set; get; }
        public int OrderNoComponent { set; get; }
        public int OrderNumbeLevel { get; set; }
        public decimal? Score { set; get; }
        public string? Grading { set; get; }
    }

}
