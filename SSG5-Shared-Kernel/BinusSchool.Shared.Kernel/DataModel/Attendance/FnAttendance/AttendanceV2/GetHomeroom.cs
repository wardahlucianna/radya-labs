using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2
{
    public class GetHomeroom
    {
        public string IdSubject { get; set; }
        public string SubjectName { get; set; }
        public string SubjectCode { get; set; }
        public string SubjectID { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string IdLesson { get; set; }
        public string IdStudent { get; set; }
        public string IdHomeroomStudent { get; set; }
        public string ClassId { get; set; }
        public ItemValueVm Homeroom { get; set; }
        public CodeWithIdVm Level { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public CodeWithIdVm AcademicYear { get; set; }
        public string IdClassroom { get; set; }
        public string ClassroomCode { get; set; }
        public string ClassroomName { get; set; }
        public int Semester { get; set; }
        public bool IsDelete { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string IdHomeroomStudentEnrollment { get; set; }
        public string BinusianID { get; set; }
        public bool IsFromMaster { get; set; }
        public bool IsShowHistory { get; set; }
        public DateTime? Datein { get; set; }
    }
}
