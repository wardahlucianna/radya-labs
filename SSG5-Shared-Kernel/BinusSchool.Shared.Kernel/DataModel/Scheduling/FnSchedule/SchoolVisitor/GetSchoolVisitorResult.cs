using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolVisitor
{
    public class GetSchoolVisitorResult : CodeWithIdVm
    {
        public string VisitorName { set; get; }
        public DateTime VisitorDate { set; get; }
        public string Venue { set; get; }
        public string UserBook { set; get; }
        public string VisitType { set; get; }
        public string Time { set; get; }
        public bool DisabledButton { set; get; }

    }
}
