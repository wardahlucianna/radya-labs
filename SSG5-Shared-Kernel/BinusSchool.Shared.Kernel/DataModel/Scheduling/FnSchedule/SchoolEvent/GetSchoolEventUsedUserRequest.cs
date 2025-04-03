using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent
{
    public class GetSchoolEventUsedUserRequest
    {
        public IEnumerable<string> IdUser { get; set; }
    }
}
