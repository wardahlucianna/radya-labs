using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterParticipant
{
    public class GetMasterParticipantResult
    {
        public NameValueVm Extracurricular { get; set; }
        public NameValueVm ExtracurricularGroup { get; set; }
        public ExtracurricularCategory Category { get; set; }
        public bool Status { get; set; }
        public ItemValueVm AcademicYear { get; set; }
        public int Semester { get; set; }
        public List<LevelGrade> LevelGradeList { get; set; }
        public List<DayTimeSchedule> ScheduleDayTimeList { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        //public int TotalSession { get; set; }
        public int ParticipantMin { get; set; }
        public int ParticipantMax { get; set; }
        public int TotalParticipant { get; set; }
        public decimal? DefaultPrice { get; set; }
    }
    
    public class DayTimeSchedule 
    {
        public string Day { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }

    public class LevelGrade
    {
        public ItemValueVm Level { get; set; }
        public ItemValueVm Grade { get; set; }
        public List<ItemValueVm> HomeroomList { get; set; }
    }
}
