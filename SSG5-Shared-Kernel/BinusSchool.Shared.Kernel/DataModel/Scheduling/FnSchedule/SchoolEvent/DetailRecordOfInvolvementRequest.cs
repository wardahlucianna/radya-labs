using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent
{
    public class DetailRecordOfInvolvementRequest : CollectionSchoolRequest
    {
        public string IdEvent { get; set; }
        public string IdUser { get; set; }
    }
}
