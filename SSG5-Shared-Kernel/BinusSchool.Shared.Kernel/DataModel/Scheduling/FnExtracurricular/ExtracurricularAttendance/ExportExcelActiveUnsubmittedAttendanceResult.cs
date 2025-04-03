using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularAttendance
{
    public class ExportExcelActiveUnsubmittedAttendanceResult
    {
        public string SchoolDescription { get; set; }
        public string TotalSupervisor { get; set; }
        public List<ExportExcelActiveUnsubmittedAttendanceResultStudent> StudentAttendanceList { get; set; }
        public List<ExportExcelActiveUnsubmittedAttendanceResultUnsubmitted> UnsubmittedAttendanceList { get; set; }

    }
    public class ExportExcelActiveUnsubmittedAttendanceResultStudent
    {
        public string AcademicYear { get; set; }
        public string Semester { get; set; }
        public string ElectiveName { get; set; }
        public string Grade { get; set; }
        public NameValueVm Student { get; set; }
        public string Class { get; set; }
        public string AttendancePercentage { get; set; }
    }

    public class ExportExcelActiveUnsubmittedAttendanceResultUnsubmitted
    {
        public string AcademicYear { get; set; }
        public string Semester { get; set; }
        public string ElectiveName { get; set; }
        public string Grade { get; set; }
        public string SupervisorCoach { get; set; }
        public string Day { get; set; }
        public DateTime Date { get; set; }
        public string TotalParticipant { get; set; }
        public string TotalEntry { get; set; }
    }
}
