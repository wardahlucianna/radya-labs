using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterRule
{
    public class GetMasterExtracurricularRuleResult : CodeWithIdVm
    {        
        public ItemValueVm AcademicYear { get; set; }
        public ItemValueVm Semester { get; set; }
        public string ExtracurricularRuleName { get; set; }
        public int MinEffectives { get; set; }
        public int MaxEffectives { get; set; }
        public bool Status { get; set; }
        public DateTime? RegistrationStartDate { get; set; }
        public DateTime? RegistrationEndDate { get; set; }
        public List<ItemValueVm> Grades { get; set; }
        public bool HasReviewDate { get; set; }
        public DateTime? ReviewDate { get; set; }
        public int DueDayPayment { get; set; }

    }
}
