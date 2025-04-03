using System;
using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterExtracurricular
{
    public class UpdateMasterExtracurricularRequest
    {
        public List<string> GradeList { get; set; }
        public string IdExtracurricular { get; set; }
        public string ExtracurricularName { get; set; }
        public string ExtracurricularDescription { get; set; }
        public string IdExtracurricularGroup { get; set; }
        public bool IsShowAttendanceReportCard { get; set; }
        public bool IsShowScoreReportCard { get; set; }
        public bool IsRegularSchedule { get; set; }
        public List<ScheduleExtracurricular> ScheduleList { get; set; }
        public DateTime AttendanceStartDate { get; set; }
        public DateTime AttendanceEndDate { get; set; }
        public DateTime ElectivesStartDate { get; set; }
        public DateTime ElectivesEndDate { get; set; }
        //public ExtracurricularCategory ExtracurricularCategory { get; set; }
        public int ParticipantMin { get; set; }
        public int ParticipantMax { get; set; }
        public DateTime? ScoringStartDate { get; set; }
        public DateTime? ScoringEndDate { get; set; }
        public List<CoachSupervisorExtracurricular> CoachSupervisorList { get; set; }
        public List<ExtCoachExtracurricularVm> ExternalCoachList { get; set; }
        public bool UpdateAll { get; set; }
        public bool Status { get; set; }
        public decimal? Price { get; set; }
        public bool NeedObjective { get; set; }
        public string IdExtracurricularScoreLegendCategory { get; set; }
        public string IdExtracurricularScoreCompCategory { get; set; }
        public string IdExtracurricularType { get; set; }
        public bool ShowParentStudent { get; set; }
    }
}
