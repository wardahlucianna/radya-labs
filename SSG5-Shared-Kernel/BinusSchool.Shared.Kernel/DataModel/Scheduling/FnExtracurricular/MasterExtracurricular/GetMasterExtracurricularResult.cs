using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterExtracurricular
{
    public class GetMasterExtracurricularResult : ItemValueVm
    {
        public string GroupName { get; set; }
        public string ExtracurricularName { get; set; }
        public string Grades { get; set; }
        public int Semester { get; set; }
        public string ScheduleDay { get; set; }
        public string ScheduleDayTime { get; set; }
        public string Supervisor { get; set; }
        public bool Status { get; set; }
        public decimal Price { get; set; }
        public bool IsAnyParticipant { get; set; }
        public DateTime? ReviewDate { get; set; }
        public bool CanChangeStatus { get; set; }
    }

    public class GetMasterExtracurricularResult_Data
    {
        public string IdElectives { get; set; }
        public string IdElectivesGroup { get; set; }
        public bool IsRegularSchedule { get; set; }
        public List<GetMasterExtracurricularResult_Grade> ElectivesGrade { get; set; }
        public List<ItemValueVm> ElectivesSession { get; set; }
    }

    public class GetMasterExtracurricularResult_Grade
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public int OrderNumber { get; set; }
    }
}
