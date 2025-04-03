using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularAttendance
{
    public class GetStudentAttendanceDetailResult
    {
        public NameValueVm Student { get; set; }
        public NameValueVm Homeroom { get; set; }
        public DateTime JoinElectiveDate { get; set; }
        public GetStudentAttendanceDetailResult_SessionAttendance SessionAttendance { get; set; }
    }

    public class GetStudentAttendanceDetailResult_SessionAttendance
    {
        public NameValueVm ExtracurricularStatusAtt { get; set; }
        public bool? NeedReason { get; set; }
        public string Reason { get; set; }
    }
}
