using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.Student
{
    public class GetStudentInformationForBNSReportRequest
    {
        public string IdStudent { get; set; }
        public string IdGrade { get; set; }
        public int Semester { get; set; }
    }

    public class GetStudentInformationForBNSReportRequest_EnrollmentStudent
    {
        public string IdSchool { get; set; }
        public string SchoolName { get; set; }
        public string SchoolDesc { get; set; }
        public string SchoolAddress { get; set; }
        public string IdAcademicYear { get; set; }
        public string AcademicYearCode { get; set; }
        public string AcademicYearDesc { get; set; }
        public string IdLevel { get; set; }
        public string LevelCode { get; set; }
        public string LevelDesc { get; set; }
        public string IndonesianLevel { get; set; }
        public string IdGrade { get; set; }
        public string GradeCode { get; set; }
        public string GradeDesc { get; set; }
        public int Semester { get; set; }
    }
}
