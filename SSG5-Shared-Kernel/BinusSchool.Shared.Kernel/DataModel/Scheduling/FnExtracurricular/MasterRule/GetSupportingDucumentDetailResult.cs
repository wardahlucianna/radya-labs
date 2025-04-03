using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterRule
{
    public class GetSupportingDucumentDetailResult : CodeWithIdVm
    {
        public ItemValueVm AcademicYear { get; set; }
        public string DocumentName { get; set; }
        public string DocumentLink { get; set; }
        public List<ItemValueVm> Grades { get; set; }
        public bool ShowToParent { get; set; }
        public bool ShowToStudent { get; set; }
        public bool Status { get; set; }
    }
}

