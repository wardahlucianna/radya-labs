using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryV2.ComponentData;
using BinusSchool.Data.Model.Scoring.FnScoring.TeacherComment;
using Org.BouncyCastle.Asn1.Mozilla;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryV2
{
    public class MasterGenerateScoreSummaryResult
    {
        public string TemplateName { get; set; }
        public string WidthItem { get; set; }
        public bool ShowSubjectLevel { set; get; }
        public bool ShowTeacherName { set; get; }
        public bool ShowTotal { set; get; }
        public GetComponentIB2023SubjectScoreResult Comp_ib_2023_subject_score { set; get; }
        public GetComponentSubjectScorePerSemesterResult Comp_subject_score_per_semester { set; get; }
        public List<GetComponentIB2023AttendanceFinalResult> Comp_ib_2023_attendance_final { set; get; }
        public List<GetComponentIB2023AttendanceSCPFinalResult> Comp_ib_2023_attendance_scp_final { set; get; }
        public GetComponentIB2023NationalRequirementsResult Comp_ib_2023_national_requirements { set; get; }
        public GetComponentMYP2023ProgressStatusResult Comp_myp_2023_progress_status { set; get; }
        public List<GetComponentMYP2023ProgressReportResult> Comp_myp_2023_progress_report { set; get; }
        public GetComponentIB2023SubjectATLResult Comp_ib_2023_subject_atl { set; get; }
        public List<GetComponentDP2023CasRsTokResult> Comp_dp_2023_cas_rs_tok { set; get; }
        public List<GetComponentDP2023GradingScoreResult> Comp_dp_2023_grading_score { set; get; }
        public GetComponentGradingScoreGroupResult Comp_grading_score_group { set; get; }
        public List<GetComponentK13_SubjectScoreWithFormulaResult> Comp_k13_subjectscorewithformula { get; set; }
        public GetComponentNatcurCharacterValueResult Comp_k13_charactervalue { set; get; }
        public List<GetTeacherCommentPerStudentResult> Comp_teacher_comments { get; set; }
        public List<GetComponentPYPPOIResult> Comp_pyp_poi { get; set; }
        public GetComponentProgressStatusResult Comp_progress_status { get; set; }
        public GetComponentAssessmentPMBenchmarkResult Comp_asmt_pmb { get; set; }
        public GetComponentSemesterScoreResult Comp_semester_score { get; set; }
        public GetComponentSubjectScoreResult Comp_subject_score { get; set; }
        public List<GetComponentSubjectScoreDetailResult> Comp_subject_score_detail { get; set; }
        public GetComponentK13NationalSubjectScoreResult Comp_k13_nationalsubjectscore { get; set; }
        public GetComponentCompAttendanceResult Comp_attendance { get; set; }
        public GetComponentGradingScoreResult Comp_grading_score { get; set; }
        public GetComponentIBTermReportResult Comp_ib_term_report { get; set; }
        public List<GetComponentSerpongAnnualProjectResult> Comp_serpong_annual_project { get; set; }
        public List<GetComponentSerpongGlobalPrespectiveResult> Comp_serpong_global_perspectives { get; set; }
        public List<GetComponentLearningContinuumResult> Comp_learning_continuum { get; set; }
        public GetComponentElectivesResult_ElectivesByYear Comp_electives_by_year { get; set; }
        public GetComponentElectivesResult_ElectivesBySemester Comp_electives_by_semester { get; set; }
        public List<GetComponentElectivesResult_ElectivesByCategory> Comp_electives_by_category { get; set; }
        public List<GetComponentElectivesResult_ElectivesByList> Comp_electives_by_list { get; set; }
        public GetComponentElectivesResult_AttendanceBySchoolDay Comp_attendance_by_school_day { get; set; }
        public string Comp_html_content { set; get; }
        public string Comp_mobile_content { set; get; }
    }
}
