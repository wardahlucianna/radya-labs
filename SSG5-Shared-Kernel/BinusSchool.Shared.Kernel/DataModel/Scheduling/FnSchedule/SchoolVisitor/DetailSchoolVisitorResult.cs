using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolVisitor
{
    public class DetailSchoolVisitorResult
    {
        public string Id { get; set; }
        public string BookFullName { get; set; }
        public string BookUserName { get; set; }
        public string BookBinusianId { get; set; }
        public string VisitorFullName { get; set; }
        public DateTime VisitorDate { get; set; }
        public ItemValueVm Vanue { get; set; }
        public string Description { get; set; }
    }
}
