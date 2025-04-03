using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolVisitor
{
    public class UpdateSchoolVisitorRequest
    {
        public string Id { get; set; }
        public DateTime VisitorDate { get; set; }
        public string IdVenue { get; set; }
        public string Description { get; set; }
    }
}
