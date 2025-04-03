using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.Student
{
    public class GetStudentInformationForBNSReportResult
    {
        public GetStudentInformationForBNSReportResult_Student Student { get; set; }
        public GetStudentInformationForBNSReportResult_ParentData Parent { get; set; }
    }

    public class GetStudentInformationForBNSReportResult_Student
    {
        public GetStudentInformationForBNSReportResult_StudentDetail StudentDetail { get; set; }
        public GetStudentInformationForBNSReportResult_StudentEnrollment StudentEnrollment { get; set; }
        public GetStudentInformationForBNSReportResult_StudentPhoto StudentPhoto { get; set; }
    }

    public class GetStudentInformationForBNSReportResult_StudentDetail
    {
        public string IdStudent { get; set; }
        public string IdBinusian { get; set; }
        public string StudentName { get; set; }
        public string NISN { get; set; }
        public string POB { get; set; }
        public string DOB { get; set; }
        public string DOBIndo { get; set; }
        public string ChildNumber { get; set; }
        public string Telephone { get; set; }
        public Gender Gender { get; set; }
        public string GenderIndo { get; set; }
        public string Religion { get; set; }
        public string ReligionIndo { get; set; }
        public string ReligionSubject { get; set; }
        public string ReligionSubjectIndo { get; set; }
        public string ChildStatus { get; set; }
        public string ChildStatusIndo { get; set; }
        public string ResidenceAddress { get; set; }
        public string JoinToSchoolIdAcademicYear { get; set; }
        public string JoinToSchoolAcademicYear { get; set; }
        public string JoinToSchoolDate { get; set; }
        public string JoinToSchoolDateIndo { get; set; }
        public string JoinToSchoolIdLevel { get; set; }
        public string JoinToSchoolIdGrade { get; set; }
        public string JoinToSchoolGrade { get; set; }
        public string PreviousSchool { get; set; }
        public string PreviousSchoolAddress { get; set; }
    }

    public class GetStudentInformationForBNSReportResult_StudentEnrollment
    {
        public GetStudentInformationForBNSReportResult_School School { get; set; }
        public CodeWithIdVm AcademicYear { get; set; }
        public CodeWithIdVm Level { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public int Semester { get; set; }
        public CodeWithIdVm Homeroom { get; set; }
        public NameValueVm HomeroomTeacher { get; set; }
        public NameValueVm HomeroomTeacher2 { get; set; }
        public NameValueVm Coordinator { get; set; }
        public NameValueVm Principal { get; set; }
        public NameValueVm PrincipalFirstYear { get; set; }
    }

    public class GetStudentInformationForBNSReportResult_ParentData
    {
        public GetStudentInformationForBNSReportResult_ParentDetail Father { get; set; }
        public GetStudentInformationForBNSReportResult_ParentDetail Mother { get; set; }
        public GetStudentInformationForBNSReportResult_ParentDetail Guardian { get; set; }
    }

    public class GetStudentInformationForBNSReportResult_ParentDetail : NameValueVm
    {
        public string IdParentRole { get; set; }
        public string Occupation { get; set; }
        public string ResidenceAddress { get; set; }
        public string Telephone { get; set; }
    }

    public class GetStudentInformationForBNSReportResult_School : NameValueVm
    {
        public string Description { get; set; }
        public string Address { get; set; }
    }

    public class GetStudentInformationForBNSReportResult_EntryGradeInSameLevel
    {
        public string EntryIdAcademicYear { get; set; }
        public string EntryAcademicYear { get; set; }
        public string EntryIdLevel { get; set; }
        public string EntryLevel { get; set; }
        public string EntryIdGrade { get; set; }
        public string EntryGrade { get; set; }
    }

    public class GetStudentInformationForBNSReportResult_StudentPhoto
    {
        public string IdAcademicYear { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
    }
}
