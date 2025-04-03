using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportData.ComponentData
{
    public class GetStudentMasterDataForSignatureReportResult
    {
        public GetStudentMasterDataForSignatureReportResult_Student Student { get; set; }
        public GetStudentMasterDataForSignatureReportResult_ParentData Parent { get; set; }
        public GetStudentMasterDataForSignatureReportResult_School School { get; set; }
        public GetStudentMasterDataForSignatureReportResult_ReportCardGradeAlias ReportCardGradeAlias { get; set; }
        public CodeWithIdVm AcademicYear { get; set; }
        public CodeWithIdVm Level { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public int Semester { get; set; }
        public string Programee { get; set; }
        public string KurnasPhase { get; set; }
        public CodeWithIdVm Homeroom { get; set; }
        public NameValueVm HomeroomTeacher { get; set; }
        public NameValueVm HomeroomTeacher2 { get; set; }
        public NameValueVm Coordinator { get; set; }
        public NameValueVm Principal { get; set; }
        public NameValueVm PrincipalFirstYear { get; set; }
        public string SchoolStartDate { get; set; }
        public string SchoolStartDateIndo { get; set; }
        public string SemesterStartDate { get; set; }
        public string SemesterStartDateIndo { get; set; }
        public string IssueDate { get; set; }
        public string IssueDateIndo { get; set; }
        public string GeneratedDate { get; set; }
        public string GeneratedDateIndo { get; set; }
        public string GraduationDate { get; set; }
        public string GraduationDateIndo { get; set; }
        public string ResultsDate { get; set; }
        public string ResultsDateIndo { get; set; }
        public string SchoolStartDay { get; set; }
        public string SchoolStartDayIndo { get; set; }
        public string SchoolAttandanceStartDay { get; set; }
        public string SchoolAttandanceStartDayIndo { get; set; }
        public string SemesterStartDay { get; set; }
        public string SemesterStartDayIndo { get; set; }
        public string IssueDay { get; set; }
        public string IssueDayIndo { get; set; }
        public string GeneratedDay { get; set; }
        public string GeneratedDayIndo { get; set; }
        public string GraduationDay { get; set; }
        public string GraduationDayIndo { get; set; }
        public string ResultsDay { get; set; }
        public string ResultsDayIndo { get; set; }
        public string SemesterAttandanceStartDate { get; set; }
        public string SemesterAttandanceStartDateIndo { get; set; }
        public string SemesterAttandanceStartDay { get; set; }
        public string SemesterAttandanceStartDayIndo { get; set; }
    }

    public class GetStudentMasterDataForSignatureReportResult_Student : NameValueVm
    {
        public string IdBinusian { get; set; }
        public string StudentPhoto { get; set; }
        public string NISN { get; set; }
        public string DOB { get; set; }
        public string DOBWithStriped { get; set; }
        public string DOBIndo { get; set; }
        public string POB { get; set; }
        public Gender Gender { get; set; }
        public string Telephone { get; set; }
        public string ChildNumber { get; set; }
        public string GenderIndo { get; set; }
        public string Religion { get; set; }
        public string ReligionIndo { get; set; }
        public string ReligionSubject { get; set; }
        public string ReligionSubjectIndo { get; set; }
        public string ChildStatus { get; set; }
        public string ChildStatusIndo { get; set; }
        public string ResidenceAddress { get; set; }
        public string JoinToSchoolDate { get; set; }
        public string JoinToSchoolDateIndo { get; set; }
        public string JoinToSchoolGrade { get; set; }
        public string JoinToSchoolAcademicYear { get; set; }
        public string PreviousSchool { get; set; }
        public string PreviousSchoolAddress { get; set; }
    }

    public class GetStudentMasterDataForSignatureReportResult_School : NameValueVm
    {
        public string Address { get; set; }
    }

    public class GetStudentMasterDataForSignatureReportResult_ParentData
    {
        public GetStudentMasterDataForSignatureReportResult_ParentDetail Father { get; set; }
        public GetStudentMasterDataForSignatureReportResult_ParentDetail Mother { get; set; }
        public GetStudentMasterDataForSignatureReportResult_ParentDetail Guardian { get; set; }
    }

    public class GetStudentMasterDataForSignatureReportResult_ParentDetail : NameValueVm
    {
        public string IdParentRole { get; set; }
        public string Occupation { get; set; }
        public string ResidenceAddress { get; set; }
        public string Telephone { get; set; }
    }

    public class GetStudentMasterDataForSignatureReportResult_ReportCardGradeAlias
    {
        public string IdGrade { get; set; }
        public string LevelName { get; set; }
        public string LevelNameIndo { get; set; }
        public string GradeName { get; set; }
        public string GradeNameIndo { get; set; }
        public string SchoolName { get; set; }
        public string SchoolNameIndo { get; set; }
    }
}
