using System;
using BinusSchool.Common.Model;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealizationV2
{
    public class DownloadSubstitutionReportV2Request
    {
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public IEnumerable<string> IdGrade { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public IEnumerable<string> IdUserTeacher { get; set; }
        public IEnumerable<string> IdUserSubstituteTeacher { get; set; }
        public string SessionID { get; set; }
        public IEnumerable<string> IdVenue { get; set; }

    }
}
