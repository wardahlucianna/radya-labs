using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry.ClinicDailyReport
{
    public class GetClinicDailyReportVisitDataResponse
    {
        public GetClinicDailyReportVisitDataResponse_Visit TotalClinicVisit { get; set; }
        public GetClinicDailyReportVisitDataResponse_Visitor TotalClinicVisitor { get; set; }
    }

    public class GetClinicDailyReportVisitDataResponse_Visit
    {
        public int Student { get; set; }
        public int Staff { get; set; }
        public int OtherPatient { get; set; }
        public int TotalVisit { get; set; }
    }

    public class GetClinicDailyReportVisitDataResponse_Visitor
    {
        public int Student { get; set; }
        public int Staff { get; set; }
        public int OtherPatient { get; set; }
        public int TotalVisitor { get; set; }
    }
}
