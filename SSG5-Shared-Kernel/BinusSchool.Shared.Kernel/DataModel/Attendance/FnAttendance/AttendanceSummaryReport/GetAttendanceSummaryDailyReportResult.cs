using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryReport
{
    public class GetAttendanceSummaryDailyReportResult
    {
        public GetAttendanceSummaryDailyReportResult_UAStudents UAStudents { set; get; }
        public GetAttendanceSummaryDailyReportResult_UAPresentStudent UAPresentStudent { set; get; }
        public GetAttendanceSummaryDailyReportResult_Summary Summary { set; get; }
    }

    public class GetAttendanceSummaryDailyReportResult_UAStudents
    {
        public List<GetAttendanceSummaryDailyReportResult_Student> ListUAStudent { set; get; }
        public List<GetAttendanceSummaryDailyReportResult_Teacher> ListTeacherAttendance { set; get; }
        public List<GetAttendanceSummaryDailyReportResult_Student> ListNotTapIn { set; get; }
    }

    public class GetAttendanceSummaryDailyReportResult_Student
    {
        public NameValueVm Student { set; get; }
        public ItemValueVm Homeroom { set; get; }
    }

    public class GetAttendanceSummaryDailyReportResult_StudentAttandance : GetAttendanceSummaryDailyReportResult_Student
    {
        public string? TappingTime { set; get; }
        public List<CodeWithIdVm> SessionAttandance { set; get; }
        public int TotalPresent { set; get; }
        public int TotalAbsent { set; get; }
        public int TotalLate { set; get; }
    }

    public class GetAttendanceSummaryDailyReportResult_StudentUAPresent : GetAttendanceSummaryDailyReportResult_Student
    {
        public CodeWithIdVm Session { set; get; }
        public NameValueVm Teacher { set; get; }
        public string ClassID { set; get; }
    }

    public class GetAttendanceSummaryDailyReportResult_Teacher
    {
        public NameValueVm Teacher { set; get; }
        public string SessionTeacher { set; get; }
    }

    public class GetAttendanceSummaryDailyReportResult_UAPresentStudent
    {
        public List<GetAttendanceSummaryDailyReportResult_StudentAttandance> ListUAPresentCheck { set; get; }
        public List<GetAttendanceSummaryDailyReportResult_StudentUAPresent> ListDetail { set; get; }
    }

    public class GetAttendanceSummaryDailyReportResult_Summary
    {
        public List<GetAttendanceSummaryDailyReportResult_PercentPerSession> ListPercentPerSession { set; get; }
        public List<GetAttendanceSummaryDailyReportResult_SummaryPerGradeSession> ListSummaryPerGradeSession { set; get; }
        public List<GetAttendanceSummaryDailyReportResult_SummaryStudentUA> ListDetail { set; get; }
    }

    public class GetAttendanceSummaryDailyReportResult_PercentPerSession
    {
        public string SessionID { set; get; }
        public int TotalUA { set; get; }
        public string Percentage { set; get; }
    }

    public class GetAttendanceSummaryDailyReportResult_SummaryPerGradeSession
    {
        public string GradeLevel { set; get; }
        public int TotalSession { set; get; }
    }

    public class GetAttendanceSummaryDailyReportResult_SummaryStudentUA : GetAttendanceSummaryDailyReportResult_Student
    {
        public string Session { set; get; }
        public string ClassID { set; get; }
        public string GradeLevel { set; get; }
    }

    public class GetAttendanceSummaryDailyReportResult_DataEnrollment
    {
        public string IdBinusian { set; get; }
        public string IdStudent { set; get; }
        public string StudentName { set; get; }
        public string IdLevel { set; get; }
        public string IdGrade { set; get; }
        public string GradeCode { set; get; }
        public string IdHomeroom { set; get; }
        public string HomeroomCode { set; get; }
        public string ClassCode { set; get; }
        public string ClassID { set; get; }
        public string IdLesson { set; get; }
    }

    public class GetAttendanceSummaryDailyReportResult_DataTransactionAttandance
    {
        public string SessionID { set; get; }
        public string SessionName { set; get; }
        public string SessionAlias { set; get; }
        public string IdTeacher { set; get; }
        public string IdBinusian { set; get; }
        public string IdStudent { set; get; }
        public string ClassID { set; get; }
        public string IdLesson { set; get; }
        public AttendanceCategory? AttendanceCategory { set; get; }
        public AbsenceCategory? AbsenceCategory { set; get; }
        public string? AttendanceDesc { set; get; }
        public DateTime? DateIn { set; get; }
    }

    public class GetAttendanceSummaryDailyReportResult_DataEnrollmentAndAttandance
    {
        public string IdBinusian { set; get; }
        public string IdStudent { set; get; }
        public string StudentName { set; get; }
        //public string IdLevel { set; get; }
        //public string IdGrade { set; get; }
        public string GradeCode { set; get; }
        public string IdHomeroom { set; get; }
        public string HomeroomCode { set; get; }
        public string ClassCode { set; get; }
        public string ClassID { set; get; }
        public string IdLesson { set; get; }
        public string IdTeacher { set; get; }
        public string TeacherName { set; get; }
        public string IdSession { set; get; }
        public string SessionID { set; get; }
        public string SessionName { set; get; }
        public string SessionAlias { set; get; }
        public AttendanceCategory? AttendanceCategory { set; get; }
        public AbsenceCategory? AbsenceCategory { set; get; }
        public string? AttendanceDesc { set; get; }
        public DateTime? TappingTime { set; get; }
    }

    public class GetAttendanceSummaryDailyReportResult_DataEnrollmentAndAttandanceBySessionName
    {
        public string IdBinusian { set; get; }
        public string IdStudent { set; get; }
        public string StudentName { set; get; }
        public string GradeCode { set; get; }
        public string IdHomeroom { set; get; }
        public string HomeroomCode { set; get; }
        public string ClassCode { set; get; }
        public string? ClassID { set; get; }
        public string? IdLesson { set; get; }
        public string? IdTeacher { set; get; }
        public string? TeacherName { set; get; }
        public string? SessionID { set; get; }
        public string SessionName { set; get; }
        public string AttendanceCode { set; get; }
        public string AttendanceDesc { set; get; }
        public string TappingTime { set; get; }
    }

    public class GetAttendanceSummaryDailyReportResult_DataTeacher
    {
        public string ClassIdGenerated { set; get; }
        public string IdLesson { set; get; }
        public string IdTeacher { set; get; }
        public string TeacherName { set; get; }
    }

    public class GetAttendanceSummaryDailyReportResult_DataTappingTransaction
    {
        public string IdBinusianCard { set; get; }
        public string IdBinusianTap { set; get; }
        public string CardIDCard { set; get; }
        public string CardIDTap { set; get; }
        public DateTime? TransactionTime { set; get; }
        public string IsActive { set; get; }
    }

    public class GetAttendanceSummaryDailyReportResult_JoinEnrollmentAndAttandance
    {
        public string IdStudent { set; get; }
        public string IdBinusian { set; get; }
        public bool IsCheck { set; get; }
        public List<GetAttendanceSummaryDailyReportResult_DataTransactionAttandance> Attandance { set; get; }
    }

    public class GetAttendanceSummaryDailyReportResult_PositionUser
    {
        public string Code { set; get; }
        public string Data { set; get; }
    }
}
