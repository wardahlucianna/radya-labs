using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry.ClinicDailyReport
{
    public class GetClinicDailyReportOfLeaveSchoolEarlyStudentResponse
    {
        public GetClinicDailyReportOfLeaveSchoolEarlyStudentResponse_Time Time { get; set; }
        public string Name { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public string Location { get; set; }
        public string Teacher { get; set; }
        public string Notes { get; set; }
    }

    public class GetClinicDailyReportOfLeaveSchoolEarlyStudentResponse_Time
    {
        public TimeSpan CheckIn { get; set; }
        public TimeSpan? CheckOut { get; set; }
    }
}
