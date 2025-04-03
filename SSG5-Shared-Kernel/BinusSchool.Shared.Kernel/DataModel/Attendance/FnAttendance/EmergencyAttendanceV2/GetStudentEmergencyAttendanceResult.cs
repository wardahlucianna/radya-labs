using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.EmergencyAttendanceV2
{
    public class GetStudentEmergencyAttendanceResult : CodeWithIdVm
    {
        public string IdEmergencyReportActived { get; set; }
        public int totalStudent { get; set; }
        public int totalStudentMarked { get; set; }
        public int totalStudentUnmarked { get; set; }
        public List<GetStudentEmergencyAttendanceResult_StudentVm> studentList { get; set; }
    }
    public class GetStudentEmergencyAttendanceResult_StudentVm
    {
        public string idEmergencyAttendance { get; set; }
        public ItemValueVm student { get; set; }
        public CodeWithIdVm level { get; set; }
        public ItemValueVm homeroom { get; set; }
        public List<GetStudentEmergencyAttendanceResult_EmergencyStatusVm> emergencyStatusList { get; set; }
        public bool isMarkFromAttendance { get; set; }
    }

  
    public class GetStudentEmergencyAttendanceResult_EmergencyStatusVm
    {
        public string idEmergencyStatus { get; set; }
        public string emergencyStatusName { get; set; }
        public bool isSelected { get; set; }
        public string description { get; set; }

    }

    public class GetStudentEmergencyAttendance_StudentVm
    {
        public string IdStudent { get; set; }
        public string StudentName { get; set; }
        public string IdLevel { get; set; }
        public string LevelName { get; set; }
        public string LevelCode { get; set; }
        public int GradeOrder { get; set; }
        public string IdHomeroom { get; set; }
        public string HomeroomName { get; set; }
    }
    public class GetStudentEmergencyAttendance_Student2Vm
    {
        public string IdStudent { get; set; }
        public string IdEmergencyAttendance { get; set; }
        public List<GetStudentEmergencyAttendanceResult_EmergencyStatusVm> emergencyStatusList { get; set; }
    }
}
