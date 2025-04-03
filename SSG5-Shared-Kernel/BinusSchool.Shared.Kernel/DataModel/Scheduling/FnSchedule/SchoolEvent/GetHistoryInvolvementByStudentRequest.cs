using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent
{
    public class GetHistoryInvolvementByStudentRequest : CollectionSchoolRequest
    {
        public string IdStudent { get; set; }
    }
}
