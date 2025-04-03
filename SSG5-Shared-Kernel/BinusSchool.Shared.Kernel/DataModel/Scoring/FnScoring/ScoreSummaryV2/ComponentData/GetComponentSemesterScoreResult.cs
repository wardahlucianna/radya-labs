using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryV2.ComponentData
{
    public class GetComponentSemesterScoreResult
    {
        public string SubjectListName { get; set; }
        public List<GetComponentSemesterScoreResult_SubjectList> SubjectList { get; set; }
        public GetComponentSemesterScoreResult_OverallScore Overall { get; set; }
    }

    public class GetComponentSemesterScoreResult_SubjectList
    {
        public string IdSubject { get; set; }
        public string SubjectName { get; set; }
        public string? TeacherName { get; set; }
        public string? Semester1Score { get; set; }
        public string? Semester2Score { get; set; }
        public string? FinalScore { get; set; }
        public string? FinalGrade { get; set; }
    }

    public class GetComponentSemesterScoreResult_OverallScore
    {
        public string? Semester1Score { get; set; }
        public string? Semester2Score { get; set; }
        public string? FinalScore { get; set; }
        public string? FinalGrade { get; set; }
    }
}
