using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryV2.ComponentData
{
    public class GetComponentAttendanceReportResult
    {
        public GetComponentElectivesResult_AttendanceBySchoolDay CompAttendanceBySchoolDay { get; set; }
    }

    public class GetComponentElectivesResult_AttendanceBySchoolDay
    {
        public string AttandanceSmt1 { get; set; }
        public string AttandanceSmt2 { get; set; }
        public string Overall { get; set; }
    }
}
