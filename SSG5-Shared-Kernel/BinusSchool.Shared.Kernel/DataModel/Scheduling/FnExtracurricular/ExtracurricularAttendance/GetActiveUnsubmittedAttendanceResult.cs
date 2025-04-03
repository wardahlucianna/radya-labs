using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularAttendance
{
    public class GetActiveUnsubmittedAttendanceResult
    {
        public int TotalSupervisor { get; set; }
        public List<GetUnsubmittedAttendanceResult_UnsubmittedList> UnsubmittedAttendanceList { get; set; }
    }

    public class GetUnsubmittedAttendanceResult_UnsubmittedList
    {        
        public string Supervisor { get; set; }
        public NameValueVm Extracurricular { get; set; }
        public DateTime ExtracurricularDate { get; set; }
        public string ExtracurricularDay { get; set; }
        public string Grade { get; set; }
        public int TotalUnsubmittedStudent { get; set; }
        public int Semester { get; set; }
    }
}
