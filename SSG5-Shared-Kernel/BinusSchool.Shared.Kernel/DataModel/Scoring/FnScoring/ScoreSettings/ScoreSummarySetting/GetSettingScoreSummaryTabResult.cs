using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryV2;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.ScoreSummarySetting
{
    public class GetSettingScoreSummaryTabResult : ItemValueVm
    {
        public CodeWithIdVm AcademicYear { get; set; }
        public List<CodeWithIdVm> Grades { get; set; }
        public List<GetSettingScoreSummaryTabResult_RoleAccessed> RoleAccessed { get; set; }
        public ItemValueVm? ReportType { get; set; }
        public int OrderNumberScoreSummaryTab { get; set; }
        public bool IsExportExcel { get; set; }
    }

    public class GetSettingScoreSummaryTabResult_RoleAccessed
    {
        public bool CanViewSummary { get; set; }
        public CodeWithIdVm Role { get; set; }
        public GetScoreSummaryTabSettingRequest RequestScoreSummaryTabSetting { get; set; }
        public MasterGenerateScoreSummaryRequest RequestMasterGenerateScoreSummary { get; set; }
    }

    public class GetSettingScoreSummaryTabResult_GetActiveAcademicYear
    {
        public string IdSchool { get; set; }
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
    }

    public class GetSettingScoreSummaryTabResult_GetHomeroomStudent
    {
        public string IdScoreSummaryTab { get; set; }
        public string IdStudent { get; set; }
        public string IdGrade { get; set; }
        public string GradeCode { get; set; }
    }

    public class GetSettingScoreSummaryTabResult_GetUserRole
    {
        public string IdRole { get; set; }
        public string IdUser { get; set; }
        public string RoleGroupCode { get; set; }
    }

    public class GetSettingScoreSummaryTabResult_TabSection
    {
        public string IdScoreSummaryTab { get; set; }
        public string ScoreSummaryTabDesc { get; set; }
        public int OrderNumberScoreSummaryTab { get; set; }
        public string IdScoreSummaryTabSection { get; set; }
        public string? IdSubjectType { get; set; }
        public int OrderNumberScoreSummaryTabSection { get; set; }
    }

    public class GetSettingScoreSummaryTabResult_ScoreSummaryTab
    {
        public string IdScoreSummaryTab { get; set; }
        public string? IdReportType { get; set; }
        public string ScoreSummaryTabDesc { get; set; }
        public int OrderNumberScoreSummaryTab { get; set; }
        public bool IsExportExcel { get; set; }
    }

    public class GetSettingScoreSummaryTabResult_ScoreSummaryTabRole
    {
        //public string IdScoreSummaryTabRole { get; set; }
        public string IdScoreSummaryTab { get; set; }
        public string IdRole { get; set; }
        public string RoleCode { get; set; }
        public string RoleDesc { get; set; }
    }

    public class GetSettingScoreSummaryTabResult_ScoreSummaryTabGrade
    {
        //public string IdScoreSummaryTabGrade { get; set; }
        public string IdScoreSummaryTab { get; set; }
        public string IdSchool { get; set; }
        public string IdAcademicYear { get; set; }
        public string AcademicYearCode { get; set; }
        public string AcademicYearDesc { get; set; }
        public string IdGrade { get; set; }
        public string GradeCode { get; set; }
        public string GradeDesc { get; set; }
        public int OrderNumberGrade { get; set; }
    }

    public class GetSettingScoreSummaryTabResult_ScoreSummaryTabSection
    {
        public string IdScoreSummaryTabSection { get; set; }
        public string IdScoreSummaryTab { get; set; }
        public int OrderNumberScoreSummaryTabSection { get; set; }
        public ItemValueVm ScoreTabTemplate { get; set; }
        public string WidthColumn { get; set; }
        public bool ShowTeacherName { get; set; }
        public bool ShowTotal { get; set; }
        public bool ShowSubjectLevel { get; set; }
        public bool HideInSemesterOne { get; set; }
        public string Content { get; set; }
        public string MobileContent { get; set; }
        public string IdSubjectType { get; set; }
    }

    public class GetSettingScoreSummaryTabResult_ScoreSummaryTabSectionSubject
    {
        public string IdScoreSummaryTab { get; set; }
        public string IdScoreSummaryTabSection { get; set; }
        public string SubjectID { get; set; }
        public int OrderNumberScoreSummaryTabSectionSubject { get; set; }
    }
}
