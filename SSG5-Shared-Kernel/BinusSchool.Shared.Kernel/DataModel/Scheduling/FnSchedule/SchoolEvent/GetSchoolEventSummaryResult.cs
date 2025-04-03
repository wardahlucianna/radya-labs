using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent
{
    public class GetSchoolEventSummaryResult : CodeWithIdVm
    {
        public string EventId { get; set; }
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public string BinusianID { get; set; }
        public string Level { get; set; }
        public string Grade { get; set; }
        public string Homeroom { get; set; }
        public string EventName { get; set; }
        public string Place { get; set; }
        public string EventActivityId { get; set; }
        public List<CodeWithIdVm> DataActivity { get; set; }
        public List<CodeWithIdVm> DataAward { get; set; }
        public string ActivityId { get; set; }
        public string ActivityName { get; set; }
        public string AwardId { get; set; }
        public string AwardName { get; set; }
        public List<CodeWithIdVm> PIC { get; set; }
        public List<CodeWithIdVm> Registrator { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
