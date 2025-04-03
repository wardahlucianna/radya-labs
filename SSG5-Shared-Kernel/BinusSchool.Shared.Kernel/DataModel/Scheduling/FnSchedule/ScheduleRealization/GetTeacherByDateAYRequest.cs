using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealization
{
    public class GetTeacherByDateAYRequest : CollectionSchoolRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public IEnumerable<string> IdGrade { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
