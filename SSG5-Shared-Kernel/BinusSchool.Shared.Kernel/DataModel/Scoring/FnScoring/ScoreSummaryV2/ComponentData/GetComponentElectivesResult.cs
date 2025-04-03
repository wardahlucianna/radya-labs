using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryV2.ComponentData
{
    public class GetComponentElectivesResult
    {
        public GetComponentElectivesResult_ElectivesByYear ElectivesByYear { get; set; }
        public GetComponentElectivesResult_ElectivesBySemester ElectivesBySemester { get; set; }
        public List<GetComponentElectivesResult_ElectivesByCategory> ElectivesByCategory { get; set; }
        public List<GetComponentElectivesResult_ElectivesByList> ElectivesByList { get; set; }
    }

    public class GetComponentElectivesResult_ElectivesByYear : GetComponentElectivesResult_ElectiveScore
    {
        public string ElectiveName { get; set; }
        //public string AttendanceElective { get; set; }
        //public string AttendanceElectiveWithDecimal { get; set; }
        //public string AttendanceElectiveWithPercent { get; set; }
        //public string AttendanceElectiveWithDecimalPercent { get; set; }
        //public string ElectiveScorePerformance { get; set; }
    }

    public class GetComponentElectivesResult_ElectivesBySemester
    {
        public string ElectiveName { get; set; }
        public GetComponentElectivesResult_ElectiveScore Semester1 { get; set; }
        public GetComponentElectivesResult_ElectiveScore Semester2 { get; set; }
    }

    public class GetComponentElectivesResult_ElectivesByCategory
    {
        public string ElectiveType { get; set; }
        public string ElectiveName { get; set; }
        public GetComponentElectivesResult_Semester Attendance { get; set; }
        public GetComponentElectivesResult_Semester Performance { get; set; }
    }

    public class GetComponentElectivesResult_ElectivesByList
    {
        public string ElectiveName { get; set; }
        public GetComponentElectivesResult_ElectiveScore Semester1 { get; set; }
        public GetComponentElectivesResult_ElectiveScore Semester2 { get; set; }
    }

    public class GetComponentElectivesResult_ElectiveType
    {
        public int OrderName { get; set; }
        public string ElectiveType { get; set; }
        public string ElectiveTypeDesc { get; set; }
    }

    public class GetComponentElectivesResult_ElectiveScore
    {
        public string Attendance { get; set; }
        public string Performance { get; set; }
    }

    public class GetComponentElectivesResult_Semester
    {
        public string Semester1 { get; set; }
        public string Semester2 { get; set; }
    }
}
