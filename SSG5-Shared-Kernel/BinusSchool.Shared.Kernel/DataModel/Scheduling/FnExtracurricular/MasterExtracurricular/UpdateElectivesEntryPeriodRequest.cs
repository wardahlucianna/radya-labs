using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterExtracurricular
{
    public class UpdateElectivesEntryPeriodRequest
    {
        public List<string> Electives { get; set; }
        public DateTime? AttendanceStartDate { get; set; }
        public DateTime? AttendanceEndDate { get; set; }
        public DateTime? ScoringStartDate { get; set; }
        public DateTime? ScoringEndDate { get; set; }
        public DateTime? ElectivesStartDate { get; set; }
        public DateTime? ElectivesEndDate { get; set; }
    }
}
