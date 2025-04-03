using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreApproval.ApprovalReportScore
{
    public class SaveApprovalReportScoreRequest
    {
        public string ApprovedBy { set; get; }
        public List<SaveApprovalReportScoreRequest_ScoreApproval> ScoreApproval { set; get; }
        public List<SaveApprovalReportScoreRequest_TeacherCommentApproval> TeacherCommentApproval { set; get; }
        public List<SaveApprovalReportScoreRequest_POI> POIApproval { get; set; }
        public List<SaveApprovalReportScoreRequest_PMBenchmark> PMBenchmarkApproval { get; set; }
    }

    public class SaveApprovalReportScoreRequest_ScoreApproval
    {
        public string IdSubjectScoreSettingApproval { set; get; }
        public string IdSubjectScoreApproval { set; get; }
        public string IdLesson { set; get; }
        public bool IsApproved { set; get; }
    }

    public class SaveApprovalReportScoreRequest_TeacherCommentApproval
    {
        public string IdCommentSettingApproval { set; get; }
        public string IdTeacherCommentApproval { set; get; }
        public string IdHomeroom { set; get; }
        public bool IsApproved { set; get; }
    }

    public class SaveApprovalReportScoreRequest_POI
    {
        public string IdProgrammeInqSettingApproval { get; set; }
        public string IdProgrammeInqApproval { get; set; }
        public string IdHomeroom { get; set; }
        public bool IsApproved { get; set; }
    }

    public class SaveApprovalReportScoreRequest_PMBenchmark
    {
        public string IdAssessmentSettingApproval { get; set; }
        public string IdAssessmentScoreApproval { get; set; }
        public string IdLesson { get; set; }
        public bool IsApproved { get; set; }
    }
}
