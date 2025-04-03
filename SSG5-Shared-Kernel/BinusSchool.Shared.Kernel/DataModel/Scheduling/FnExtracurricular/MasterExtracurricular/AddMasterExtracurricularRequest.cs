using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterExtracurricular
{
    public class AddMasterExtracurricularRequest
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public List<string> GradeList { get; set; }
        public string ExtracurricularName { get; set; }
        public string IdExtracurricularGroup { get; set; }
        public string ExtracurricularDescription { get; set; }
        public bool IsShowAttendanceReportCard { get; set; }
        public bool IsShowScoreReportCard { get; set; }
        public bool IsRegularSchedule { get; set; }
        public List<ScheduleExtracurricular> ScheduleList { get; set; }
        public DateTime ElectivesStartDate { get; set; }
        public DateTime ElectivesEndDate { get; set; }
        public DateTime AttendanceStartDate { get; set; }
        public DateTime AttendanceEndDate { get; set; }
        //public ExtracurricularCategory ExtracurricularCategory { get; set; }
        public int ParticipantMin { get; set; }
        public int ParticipantMax { get; set; }
        public DateTime? ScoringStartDate { get; set; }
        public DateTime? ScoringEndDate { get; set; }
        public decimal Price { get; set; }
        public bool NeedObjective { get; set; }

        public List<CoachSupervisorExtracurricular> CoachSupervisorList { get; set; }
        public List<ExtCoachExtracurricularVm> ExternalCoachList { get; set; }
        public string IdExtracurricularScoreLegendCategory { get; set; }
        public string IdExtracurricularScoreCompCategory { get; set; }
        public string IdExtracurricularType { get; set; }
        public bool ShowParentStudent { get; set; }
    }

    public class ScheduleExtracurricular
    {
        public ItemValueVm Day { get; set; }
        public ItemValueVm Venue { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }

    public class CoachSupervisorExtracurricular
    {
        public string IdUser { get; set; }
        //public bool IsCoach { get; set; }
        public string IdExtracurricularCoachStatus { get; set; }
    }
    public class ExtCoachExtracurricularVm
    {
        public string IdExtracurricularExternalCoach { get; set; }        
    }

}
