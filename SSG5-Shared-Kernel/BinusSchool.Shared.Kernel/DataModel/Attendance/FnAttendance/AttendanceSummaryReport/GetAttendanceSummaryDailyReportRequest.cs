using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2;
using BinusSchool.Data.Model.Attendance.FnAttendanceLongrun.Longrun.AttendanceSummaryTerm;
using NPOI.SS.UserModel;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryReport
{
    public class GetAttendanceSummaryDailyReportRequest
    {
        public string IdUser { get; set; }
        public string IdSchool { get; set; }
        public string IdAcademicYear { get; set; }
        public DateTime AttendanceDate { get; set; }
        public IEnumerable<string> Levels { get; set; }
    }

    public class GetAttendanceSummaryDailyReportRequest_ExcelSheet : DefaultSheetRequest
    {
        public ICellStyle CustomStyle { get; set; }
        public byte[] Logo { get; set; }
        public DateTime Date { get; set; }
        public GetAttendanceSummaryDailyReportResult AttendanceSummaryDailyReport { get; set; }

    }
}
