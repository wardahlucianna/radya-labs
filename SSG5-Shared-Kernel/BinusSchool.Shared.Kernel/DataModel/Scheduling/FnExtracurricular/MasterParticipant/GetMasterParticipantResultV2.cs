using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterParticipant
{
    public class GetMasterParticipantResultV2
    {
        public NameValueVm Extracurricular { get; set; }
        public NameValueVm ExtracurricularGroup { get; set; }
        public ExtracurricularCategory Category { get; set; }
        public bool Status { get; set; }
        public ItemValueVm AcademicYear { get; set; }
        public int Semester { get; set; }
        public List<LevelGradeV2> LevelGradeList { get; set; }
        public int GradeOrderNo { get; set; }
        public List<DayTimeScheduleV2> ScheduleDayTimeList { get; set; }
        public string IdDay { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        //public int TotalSession { get; set; }
        public int ParticipantMin { get; set; }
        public int ParticipantMax { get; set; }
        public int TotalParticipant { get; set; }
        public decimal? DefaultPrice { get; set; }
    }

    public class LevelGradeV2
    {
        public ItemValueVm Level { get; set; }
        public ItemValueVm Grade { get; set; }
        public List<ItemValueVm> HomeroomList { get; set; }
        public int GradeOrderNo { get; set; }
    }

    public class DayTimeScheduleV2
    {
        public string DayName { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string IdDay { get; set; }
    }
}
