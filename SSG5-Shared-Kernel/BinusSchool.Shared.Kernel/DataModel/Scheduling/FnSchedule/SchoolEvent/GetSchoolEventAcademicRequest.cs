using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent
{
    public class GetSchoolEventAcademicRequest
    {
        public string IdAcadyear { get; set; }
        public string IdLevel { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}