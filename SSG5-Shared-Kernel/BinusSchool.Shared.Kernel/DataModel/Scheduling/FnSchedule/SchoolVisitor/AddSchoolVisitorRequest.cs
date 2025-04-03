using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolVisitor
{
    public class AddSchoolVisitorRequest
    {
        public string IdUserBook { get; set; }
        public string NameBook { get; set; }
        public string IdUserVisitor { get; set; }
        public string NameVisitor { get; set; }
        public DateTime VisitorDate { get; set; }
        public string IdVenue { get; set; }
        public string Description { get; set; }
        public string IdAcademicYear { get; set; }
    }
}
