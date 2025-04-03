using System;
using BinusSchool.Common.Model;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealizationV2
{
    public class GetListScheduleRealizationByTeacherV2Request : CollectionSchoolRequest
    {
        public string IdAcademicYear { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string IdUserTeacher { get; set; }
        public IEnumerable<string> ClassID { get; set; }
    }
}
