using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterExtracurricular
{
    public class GetMasterExtracurricularDetailResult
    {
        public ItemValueVm AcademicYear { get; set; }
        public List<ItemValueVm> Level { get; set; }
        public List<ItemValueVm> Grade { get; set; }
        public ItemValueVm Semester { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public ItemValueVm ExtracurricularGroup { get; set; }
        public string Description { get; set; }
        public bool ShowAttendanceRC { get; set; }
        public bool ShowScoreRC { get; set; }
        public bool IsRegularSchedule { get; set; }
        public List<ScheduleExtracurricularDetail> ScheduleList { get; set; }
        public DateTime ElectivesStartDate { get; set; }
        public DateTime ElectivesEndDate { get; set; }
        public DateTime AttendanceStartDate { get; set; }
        public DateTime AttendanceEndDate { get; set; }
        //public ItemValueVm Category { get; set; }
        public int MinParticipant { get; set; }
        public int MaxParticipant { get; set; }
        public DateTime? ScoreStartDate { get; set; }
        public DateTime? ScoreEndDate { get; set; }
        public decimal Price { get; set; }
        public bool NeedObjective { get; set; }
        public DateTime? ReviewDate { get; set; }
        public bool InRegistrationPeriod { get; set; }
        public List<CoachSupervisorExtracurricularDetail> CoachSupervisorList { get; set; }
        public List<ExternalCoachExtracurricularDetail> ExternalCoachList { get; set; }
        public ItemValueVm ScoreComponentCategory { get; set; }
        public ScoreLegendMappingExtracurricularDetail ScoreLegendMapping { get; set; }
        public ItemValueVm ExtracurricularType { get; set; }
        public bool? ShowParentStudent { get; set; }
    }

    public class ScheduleExtracurricularDetail
    {
        public string Id { get; set; }
        public ItemValueVm Day { get; set; }
        public ItemValueVm Venue { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }

    public class CoachSupervisorExtracurricularDetail
    {
        public string Id { get; set; } //Id Binusian
        public string Description { get; set; } //Nama Binusian
        public bool IsSpv { get; set; }
        public string IdExtracurricularCoachStatus { get; set; }
        public string ExtracurricularCoachStatusDesc { get; set; }
    }

    public class ExternalCoachExtracurricularDetail
    {
        public string Id { get; set; } //Id Binusian
        public string Description { get; set; } //Nama Binusian       
    }

    public class ScoreLegendMappingExtracurricularDetail : ItemValueVm
    {
        public string IdExtracurricularScoreLegendCategory { get; set; }
    }
}
