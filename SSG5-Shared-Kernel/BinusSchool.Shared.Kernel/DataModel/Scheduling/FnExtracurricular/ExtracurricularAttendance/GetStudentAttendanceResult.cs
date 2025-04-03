using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularAttendance
{
    public class GetStudentAttendanceResult
    {
        public NameValueVm Extracurricular { get; set; }
        public bool IsRegularSchedule { get; set; }
        public List<NameValueVm> Supervisor { get; set; }
        public List<NameValueVm> Coach { get; set; }
        public int TotalSessionCurrentMonth { get; set; }
        public int MaxSession { get; set; }
        public DateTime? ElectivesStartDate { get; set; }
        public DateTime? ElectivesEndDate { get; set; }
        public DateTime? AttendanceStartDate { get; set; }
        public DateTime? AttendanceEndDate { get; set; }
        public List<GetStudentAttendanceResult_Header> HeaderList { get; set; }
        public List<GetStudentAttendanceResult_Body> BodyList { get; set; }
    }

    public class GetStudentAttendanceResult_Header
    {
        public NameValueVm ExtracurricularGeneratedAtt { get; set; }
        public DateTime ExtracurricularGeneratedDate { get; set; }
        public string LastUpdated { get; set; }
    }

    public class GetStudentAttendanceResult_Body
    {
        public NameValueVm Student { get; set; }
        public NameValueVm Homeroom { get; set; }
        public DateTime JoinElectiveDate { get; set; }
        public List<GetStudentAttendanceResult_SessionAttendance> SessionAttendanceList { get; set; }
    }

    public class GetStudentAttendanceResult_SessionAttendance
    {
        public string IdExtracurricularGeneratedAtt { get; set; }
        public DateTime? ExtracurricularGeneratedAttDate { get; set; }
        public NameValueVm ExtracurricularStatusAtt { get; set; }
        public bool? NeedReason { get; set; }
        public string Reason { get; set; }
        public bool  NoNeedAttendance { get; set; }
    }
}
