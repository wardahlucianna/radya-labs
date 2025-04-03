using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreApproval.ApprovalReportScore
{
    public class GetApprovalReportScoreResult
    {
        public string FileNameExcel { get; set; }
        public DateTime StartExecuteDate { get; set; }
        public List<GetApprovalReportScoreResult_Score> Score { set; get; }
        public List<CodeWithIdVm> ScoreApprovalHeader { set; get; }
        public List<GetApprovalReportScoreResult_TeacherComment> TeacherComment { set; get; }
        public List<CodeWithIdVm> TeacherCommentHeader { set; get; }
        public List<GetApprovalReportScoreResult_POI> POI { get; set; }
        public List<CodeWithIdVm> POIHeader { get; set; }
        public List<GetApprovalReportScoreResult_PMBenchmark> PMBenchmark { get; set; }
        public List<CodeWithIdVm> PMBenchmarkHeader { get; set; }
    }

    public class GetApprovalReportScoreResult_Score
    {
        public ItemValueVm Subject { set; get; }
        public string IdSubjectLevel { set; get; }
        public string IdAcademicYear { set; get; }
        public int Semester { set; get; }
        public ItemValueVm Period { set; get; }
        public ItemValueVm Lesson { set; get; }
        public ItemValueVm Teacher { set; get; }
        public int ScoreStatus { set; get; }
        public int BlankScore { set; get; }
        public int ZeroScore { set; get; }
        public bool IsLocked { set; get; }
        public List<GetApprovalReportScoreResult_ScoreApproval> ListApproval { set; get; }
    }

    public class GetApprovalReportScoreResult_ScoreApproval
    {
        public string IdTeacherPosition { set; get; }
        public string TeacherPositionCode { set; get; }
        public string TeacherPositionName { set; get; }
        public string IdSubjectScoreApproval { set; get; }
        public string IdSubjectScoreSettingApproval { set; get; }
        public string IdLesson { set; get; }
        public bool IsApproved { set; get; }
        public bool IsCanApprove { set; get; }
        public bool IsCanEdit { set; get; }
        public int OrderNumber { set; get; }
    }

    public class GetApprovalReportScoreResult_EnrollmentJoinSettingScoreApproval
    {
        public string IdSubject { set; get; }
        public string SubjectName { set; get; }
        public string SubjectID { set; get; }
        public string ClassIdGenerated { set; get; }
        public string IdSubjectLevel { set; get; }
        public string IdLesson { set; get; }
        public int Semester { set; get; }
        public string IdPeriod { set; get; }
        public string? IdSubjectScoreSettingApproval { set; get; }
        public string? IdSubjectScoreSetting { set; get; }
        public string? IdTeacherPosition { set; get; }
        public string? TeacherPositionName { set; get; }
        public string? TeacherPositionCode { set; get; }
        public bool? IsEditable { set; get; }
        public bool? IsFinalApprover { set; get; }
        public int? OrderNumber { set; get; }
        public string? IdSubComponent { get; set; }
        public bool? EnableTeacherJudgement { get; set; }

    }

    public class GetApprovalReportScoreResult_SettingScoreApproval
    {
        public string IdPeriod { set; get; }
        public string PeriodCode { set; get; }
        public string IdSubjectScoreSettingApproval { set; get; }
        public string IdSubjectScoreSetting { set; get; }
        public string IdSubject { set; get; }
        public string IdSubjectLevel { set; get; }
        public string IdTeacherPosition { set; get; }
        public string TeacherPositionName { set; get; }
        public string TeacherPositionCode { set; get; }
        public bool IsEditable { set; get; }
        public bool IsFinalApprover { set; get; }
        public int OrderNumber { set; get; }
        public bool IsInPeriod { get; set; }
    }

    public class GetApprovalReportScoreResult_TransactionScoreApproval
    {
        public string IdTeacherPosition { set; get; }
        public string TeacherPositionName { set; get; }
        public string TeacherPositionCode { set; get; }
        public string IdSubjectScoreApproval { set; get; }
        public string IdSubjectScoreSettingApproval { set; get; }
        public string IdLesson { set; get; }
        public bool IsApproved { set; get; }
    }

    public class GetApprovalReportScoreResult_TeacherComment
    {
        public string IdAcademicYear { set; get; }
        public string IdHomeroom { set; get; }
        public int Semester { set; get; }
        public ItemValueVm Period { set; get; }
        public ItemValueVm Classroom { set; get; }
        public ItemValueVm Teacher { set; get; }
        public int ScoreStatus { set; get; }
        public bool IsLocked { set; get; }
        public List<GetApprovalReportScoreResult_TeacherCommentApproval> ListApproval { set; get; }
    }

    public class GetApprovalReportScoreResult_TeacherCommentApproval
    {
        public string IdTeacherPosition { set; get; }
        public string TeacherPositionCode { set; get; }
        public string TeacherPositionName { set; get; }
        public string IdTeacherCommentApproval { set; get; }
        public string IdCommentSettingApproval { set; get; }
        public string IdHomeroom { set; get; }
        public bool IsApproved { set; get; }
        public bool IsCanApprove { set; get; }
        public bool IsCanEdit { set; get; }
        public int OrderNumber { set; get; }
    }

    public class GetApprovalReportScoreResult_POI
    {
        public string IdAcademicYear { set; get; }
        public string IdHomeroom { set; get; }
        public int Semester { set; get; }
        public ItemValueVm Period { set; get; }
        public ItemValueVm Classroom { set; get; }
        public ItemValueVm Teacher { set; get; }
        public int ScoreStatus { set; get; }
        public bool IsLocked { set; get; }
        public List<GetApprovalReportScoreResult_POIApproval> ListApproval { set; get; }
    }

    public class GetApprovalReportScoreResult_POIApproval
    {
        public string IdTeacherPosition { set; get; }
        public string TeacherPositionCode { set; get; }
        public string TeacherPositionName { set; get; }
        public string IdProgrammeInqSettingApproval { set; get; }
        public string IdProgrammeInqApproval { set; get; }
        public string IdHomeroom { set; get; }
        public bool IsApproved { set; get; }
        public bool IsCanApprove { set; get; }
        public bool IsCanEdit { set; get; }
        public int OrderNumber { set; get; }
    }

    public class GetApprovalReportScoreResult_PMBenchmark
    {
        public string IdAcademicYear { set; get; }
        public int Semester { set; get; }
        public ItemValueVm Period { set; get; }
        public ItemValueVm Lesson { set; get; }
        public ItemValueVm Teacher { set; get; }
        public ItemValueVm AssessmentType { get; set; }
        public int ScoreStatus { set; get; }
        public bool IsLocked { set; get; }
        public List<GetApprovalReportScoreResult_PMBenchmarkApproval> ListApproval { set; get; }
    }

    public class GetApprovalReportScoreResult_PMBenchmarkApproval
    {
        public string IdTeacherPosition { set; get; }
        public string TeacherPositionCode { set; get; }
        public string TeacherPositionName { set; get; }
        public string IdAssessmentSettingApproval { set; get; }
        public string IdAssessmentScoreApproval { set; get; }
        public string IdLesson { set; get; }
        public bool IsApproved { set; get; }
        public bool IsCanApprove { set; get; }
        public bool IsCanEdit { set; get; }
        public int OrderNumber { set; get; }
    }

    public class GetApprovalReportScoreResult_AssessmentSettingApproval
    {
        public string IdAssessmentSettingApproval { set; get; }
        public string IdAssessmentSetting { set; get; }
        public string IdGrade { set; get; }
        public string Semester { set; get; }
        public string AssessmentType { set; get; }
        public string IdPeriod { set; get; }
        public bool IsEditable { set; get; }
        public bool IsFinalApprover { set; get; }
        public string IdTeacherPosition { set; get; }
        public string TeacherPositionCode { set; get; }
        public string TeacherPositionName { set; get; }
        public int OrderNumber { set; get; }
        public bool IsInPeriod { set; get; }
    }

    public class GetApprovalReportScoreResult_AssessmentSettingLesson
    {
        public string IdLesson { set; get; }
        public string ClassIdGenerated { set; get; }
        public string IdAssessmentType { set; get; }
        public string AssessmentTypeLongDesc { set; get; }
        public string AssessmentType { set; get; }
        public string IdPeriod { set; get; }
        public bool IsEditable { set; get; }
        public bool IsFinalApprover { set; get; }
        public string IdTeacherPosition { set; get; }
        public string TeacherPositionCode { set; get; }
        public string TeacherPositionName { set; get; }
        public int OrderNumber { set; get; }
        public bool IsInPeriod { set; get; }
    }

    public class GetApprovalReportScoreResult_SubjectSetting
    {
        public string IdSubject { get; set; }
        public string IdSubjectLevel { get; set; }
        public string IdLesson { set; get; }
        public string SubjectID { set; get; }
        public string ClassIdGenerated { set; get; }
        public string IdSubjectScoreSetting { get; set; }
        public string IdPeriod { set; get; }
        //public decimal MaxScore { set; get; }
        //public decimal MinScore { set; get; }
        public string IdSubComponentCounter { get; set; }
        public string IdSubComponent { set; get; }
        public bool EnableTeacherJudgement { set; get; }
        public bool EnablePredictedGrade { set; get; }
    }
}
