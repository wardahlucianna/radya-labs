using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry.ClinicDailyReport
{
    public class GetClinicDailyReportExcludeInjuryVisitReponse
    {
        public GetClinicDailyReportExcludeInjuryVisitReponse_Visit Summary { get; set; }
        public List<GetClinicDailyReportExcludeInjuryVisitReponse_Incident> Incidents { get; set; }
    }

    public class GetClinicDailyReportExcludeInjuryVisitReponse_Visit
    {
        public int TotalOccurrence { get; set; }
        public int UniqueIndividual { get; set; }
    }

    public class GetClinicDailyReportExcludeInjuryVisitReponse_Incident
    {
        public GetClinicDailyReportExcludeInjuryVisitReponse_Incident_Time Time { get; set; }
        public string Name { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public string Location { get; set; }
        public string Teacher { get; set; }
        public string Notes { get; set; }
        public class GetClinicDailyReportExcludeInjuryVisitReponse_Incident_Time
        {
            public TimeSpan CheckIn { get; set; }
            public TimeSpan? CheckOut { get; set; }
        }
    }
}
