using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.EmergencyAttendanceV2
{
    public class GetEmergencyReportDetailResult
    {
        public string idEmergencyAttendance { get; set; }
        public ItemValueVm student { get; set; }
        public string levelName { get; set; }
        public string homeroomName { get; set; }
        public List<GetEmergencyReportDetailResult_EmergencyStatusVm> emergencyStatusList { get; set; }
    }

    public class GetEmergencyReportDetailResult_EmergencyStatusVm
    {
        public string idEmergencyStatus { get; set; }
        public string emergencyStatusName { get; set; }
        public bool isSelected { get; set; }
        public string description { get; set; }
        public string markBy {  get; set; }
        public string markStatus { get; set; }
    }
    public class GetEmergencyReportDetailResult_StudentVm
    {
        public string IdHomeroom { get; set; }
        public string HomeroomName { get; set; }
        public string IdLevel { get; set; }
        public string LevelName { get; set; }
        public string IdGrade { get; set; }
        public string GradeName { get; set; }
        public string IdStudent { get; set; }
        public string StudentName { get; set; }

    }
}
