using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent
{
    public class GetDataActivityForMeritRequest : CollectionSchoolRequest
    {
        public string IdEvent { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdClassroom { get; set; }
        public string IdHomeroom { get; set; }
        public string IdActivity { get; set; }
        public string IdAward { get; set; }
    }
}
