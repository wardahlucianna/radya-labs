using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm
{
    public class GetAttendanceSummaryUnsubmitedResult : CodeWithIdVm
    {
        public DateTime Date { get; set; }
        public string ClassID { get; set; }
        public ItemValueVm Teacher { get; set; }
        public ItemValueVm HomeroomTeacher { get; set; }
        public ItemValueVm Homeroom { get; set; }
        public RedisAttendanceSummarySession Session { get; set; }
        public string SubjectId { get; set; }
        public int TotalStudent { get; set; }
        public List<string> ListStudent {  get; set; }
        public List<AttendanceSummaryUnsubmitedDetail> ListStudentDetail {  get; set; }
    }

    public class AttendanceSummaryUnsubmitedDetail
    {
        public string Homeroom { get; set; }
        public string ClassId { get; set; }
        public string Session { get; set; }
        public string StudentName { get; set; }
        public string IdStudent { get; set; }
        public string Status { get; set; }
    }
}
