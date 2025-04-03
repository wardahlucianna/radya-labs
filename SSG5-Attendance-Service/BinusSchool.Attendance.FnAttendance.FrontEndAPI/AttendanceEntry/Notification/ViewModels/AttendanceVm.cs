using System;
using System.Collections.Generic;

namespace BinusSchool.Attendance.FnAttendance.AttendanceEntry.Notification.ViewModels
{
    public class UserAttendanceVm<T>
    {
        public string IdUser { get; set; }
        public string EmailAddress { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<T> Attendances { get; set; }
    }
    public class ParentAttendanceVm<T>
    {
        public string IdUser { get; set; }
        public string EmailAddress { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public T StudentAttendance { get; set; }
    }
    public class PendingAttendanceVm
    {
        public DateTime Date { get; set; }
        public string HomeroomName { get; set; }
        public string IdSession { get; set; }
        public string SessionId { get; set; }
        public string ClassId { get; set; }
        public string LinkAttendance { get; set; }
    }
    public class UnsubmitSessionAttendanceVm
    {
        public DateTime Date { get; set; }
        public string Subject { get; set; }
        public string HomeroomName { get; set; }
        public string IdSession { get; set; }
        public string SessionId { get; set; }
        public string ClassId { get; set; }
        public string IdUserTeacher { get; set; }
        public int countStudent { get; set; }
        public string LinkAttendance { get; set; }
    }
    public class UnsubmitDayAttendanceVm
    {
        public DateTime Date { get; set; }
        public string IdHomeroom { get; set; }
        public string IdUserTeacher { get; set; }
        public string HomeroomName { get; set; }
        public string LinkAttendance { get; set; }
    }
    public class SubmittedDayAttendanceVm
    {
        public string BinusianId { get; set; }
        public string StudentName { get; set; }
        public List<string> Weeks { get; set; }
        public List<string> ClassIds { get; set; }
        public DateTime Date { get; set; }
        public string AttendanceName { get; set; }
        public DateTime? DateIn { get; set; }
    }

    public class LateDayAttendanceVm
    {
        public string StudentName { get; set; }
        public string BinusianId { get; set; }
        public string Grade { get; set; }
        public string Homeroom { get; set; }
        public DateTime Date { get; set; }
        public string AttendanceName { get; set; }
        public TimeSpan? LateTime { get; set; }
        public string FileLink { get; set; }
        public string Notes { get; set; }
        public string AbsentBy { get; set; }
    }
    public class LateSessionAttendanceVm
    {
        public string StudentName { get; set; }
        public string BinusianId { get; set; }
        public string Grade { get; set; }
        public string Homeroom { get; set; }
        public DateTime Date { get; set; }
        public string ClassId { get; set; }
        public string SessionId { get; set; }
        public string AttendanceName { get; set; }
        public TimeSpan? LateTime { get; set; }
        public string FileLink { get; set; }
        public string Notes { get; set; }
        public string AbsentBy { get; set; }
    }
    public class AbsentDayAttendanceVm
    {
        public string StudentName { get; set; }
        public string BinusianId { get; set; }
        public string Grade { get; set; }
        public string Homeroom { get; set; }
        public DateTime Date { get; set; }
        public string AttendanceCategory { get; set; }
        public string AbsenceCategory { get; set; }
        public string AttendanceName { get; set; }
        public string FileLink { get; set; }
        public string Notes { get; set; }
        public string AbsentBy { get; set; }
    }
    public class AbsentSessionAttendanceVm
    {
        public string StudentName { get; set; }
        public string BinusianId { get; set; }
        public string Grade { get; set; }
        public string Homeroom { get; set; }
        public DateTime Date { get; set; }
        public string ClassId { get; set; }
        public string SessionId { get; set; }
        public string AttendanceCategory { get; set; }
        public string AbsenceCategory { get; set; }
        public string AttendanceName { get; set; }
        public string FileLink { get; set; }
        public string Notes { get; set; }
        public string AbsentBy { get; set; }
    }
    public class UpdateDayAttendanceVm
    {
        public UpdateDayAttendanceDataVm OldData { get; set; }
        public UpdateDayAttendanceDataVm NewData { get; set; }
    }
    public class UpdateDayAttendanceDataVm
    {
        public string StudentName { get; set; }
        public string BinusianId { get; set; }
        public string Grade { get; set; }
        public string Homeroom { get; set; }
        public DateTime Date { get; set; }
        public string AttendanceCategory { get; set; }
        public string AbsenceCategory { get; set; }
        public string AttendanceName { get; set; }
        public string FileLink { get; set; }
        public string Notes { get; set; }
        public string AbsentBy { get; set; }
    }
    public class UpdateSessionAttendanceVm
    {
        public UpdateSessionAttendanceDataVm OldData { get; set; }
        public UpdateSessionAttendanceDataVm NewData { get; set; }
    }
    public class UpdateSessionAttendanceDataVm
    {
        public string StudentName { get; set; }
        public string BinusianId { get; set; }
        public string Grade { get; set; }
        public string Homeroom { get; set; }
        public DateTime Date { get; set; }
        public string ClassId { get; set; }
        public string SessionId { get; set; }
        public string AttendanceCategory { get; set; }
        public string AbsenceCategory { get; set; }
        public string AttendanceName { get; set; }
        public string FileLink { get; set; }
        public string Notes { get; set; }
        public string AbsentBy { get; set; }
    }
}
