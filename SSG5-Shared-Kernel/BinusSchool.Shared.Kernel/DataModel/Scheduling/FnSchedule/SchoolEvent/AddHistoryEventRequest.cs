using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent
{
    public class AddHistoryEventRequest
    {
        public string IdEvent { get; set; }
        public string IdUser { get; set; }
        public string ActionType { get; set; }
    }
}
