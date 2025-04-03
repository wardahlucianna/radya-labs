using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolVisitor
{
    public class GetSchoolVisitorRequest : CollectionSchoolRequest
    {
        public string IdAcademicYear { get; set; }
        public string VisitorType { get; set; }
        public DateTime VisitDate { get; set; }
    }
}
