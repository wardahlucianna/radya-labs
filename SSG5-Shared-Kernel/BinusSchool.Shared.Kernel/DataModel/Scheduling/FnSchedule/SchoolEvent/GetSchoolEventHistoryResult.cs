using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent
{
    public class GetSchoolEventHistoryResult : CodeWithIdVm
    {
        public string IdEvent { get; set; }
        public string ChangeNotes { get; set; }
        public DateTime? ChangetDate { get; set; }
    }
}
