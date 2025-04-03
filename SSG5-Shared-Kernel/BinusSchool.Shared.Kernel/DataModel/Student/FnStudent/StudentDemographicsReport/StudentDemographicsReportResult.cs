using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport
{
    public class StudentDemographicsReportResult
    {
        public GetSDRTotalStudentReportsResult SDRTotalStudentReports { get; set; }
        public List<GetSDRTotalStudentReportDetailsResult> SDRTotalStudentReportDetails { get; set; }
        //public GetSDRReligionReportsResult SDRReligions { get; set; }
        //public List<GetSDRReligionReportDetailsResult> SDRReligionDetails { get; set; }
    }

    public class StudentDemographicsReportResult_Homeroom
    {
        public string IdLevel { get; set; }
        public string LevelName { get; set; }
        public string IdGrade { get; set; }
        public string GradeName { get; set; }
        public string GradeCode { get; set; }
        public int Semester { get; set; }
        public string Term { get; set; }
        public string IdHomeroom { get; set; }
        public string HomeroomName { get; set; }
        public DateTime? StartDate { set; get; }
        public DateTime? EndDate { set; get; }

    }

    public class StudentDemographicsReportResult_HomeroomStudentAdmissionData
    {
        public string IdLevel { get; set; }
        public string LevelName { get; set; }
        public string IdGrade { get; set; }
        public string GradeName { get; set; }
        public string GradeCode { get; set; }
        public string IdHomeroom { get; set; }
        public string HomeroomName { get; set; }
        public string IdStudent { set; get; }
        public string StudentName { set; get; }
        public int IdStudentStatus { get; set; }
        public string? IdStudentAdmission { set; get; }
        public DateTime? JoinToSchoolDate { get; set; }
        public int Semester { get; set; }
    }

    public class StudentDemographicsReportResult_StudentDetail
    {
        public ItemValueVm Student { get; set; }
        public ItemValueVm Level { get; set; }
        public ItemValueVm Grade { get; set; }
        public ItemValueVm Homeroom { get; set; }
        public ItemValueVm HomeroomTeacher { get; set; }
        public ItemValueVm Streaming { get; set; }
        public string JoinToSchoolDate { get; set; }
        public string StudentReligion { get; set; }
        public ItemValueVm ReligionSubject { get; set; }
    }
}
