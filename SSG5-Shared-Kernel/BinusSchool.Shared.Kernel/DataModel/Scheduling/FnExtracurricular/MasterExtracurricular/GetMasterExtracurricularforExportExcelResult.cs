using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterExtracurricular
{
    public class GetMasterExtracurricularforExportExcelResult
    {
        public string ExtracurricularName { get; set; }
        public string ExtracurricularGroup { get; set; }
        public string Description { get; set; }
        public string Grade { get; set; }
        public string ShowAttendance { get; set; }
        public string ShowScore { get; set; }
        public string Schedule { get; set; }
        public string Venue { get; set; }
        public string ElectivesDate { get; set; }
        public ExtracurricularCategory Category { get; set; }
        public string Participant { get; set; }
        public string ScoringDate { get; set; }
        public string Supervisor { get; set; }
        public string Coach { get; set; }
        public string ExtCoach { get; set; }
        public decimal Price { get; set; }
    }
}
