using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent
{
    public class SetSchoolEventApprovalStatusRequest
    {
        public string Id { get; set; }
        public string IdEvent { get; set; }
        public string IdUser { get; set; }
        public bool IsSectionEventSetting { get; set; }
        public bool IsApproved  { get; set; }
        public bool IsShowAcademicCalender  { get; set; }
        public string Reason { get; set; }
    }
}