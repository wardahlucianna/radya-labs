using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class RedisAttendanceSummaryHomeroomStudentEnrollmentResult
    {
        public string IdLesson { get; set; }
        public string IdHomeroomStudent { get; set; }
        public string ClassId { get; set; }

        public RedisAttendanceSummaryStudent Student { get; set; }
        public ItemValueVm Homeroom { get; set; }
        public CodeWithIdVm Level { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public CodeWithIdVm Classroom { get; set; }
        public RedisAttendanceSummarySubject Subject { get; set; }
        public int Semester { get; set; }
        public bool IsFromMaster { get; set; }
        public bool IsDelete { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string IdHomeroomStudentEnrollment { get; set; }
    }

    public class RedisAttendanceSummaryStudent
    {
        public string IdStudent { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
    }
}
