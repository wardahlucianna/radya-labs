using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterRule
{
    public class UpdateMasterExtracurricularRuleRequest
    {
        public bool ActionUpdateStatus { get; set; }
        public string IdExtracurricularRule { get; set; }
        public string IdAcademicYear { get; set; }
        public bool BothSemester { get; set; }
        public int Semester { get; set; }
        public string Name { get; set; }
        public int MinEffectives { get; set; }
        public int MaxEffectives { get; set; }
        public DateTime? RegistrationStartDate { get; set; }
        public DateTime? RegistrationEndDate { get; set; }
        public bool Status { get; set; }
        public List<ItemValueVm> Grades { get; set; }
        public DateTime? ReviewDate { get; set; }
        public int DueDayPayment { get; set; }
        public bool IsEnableReviewDate { get; set; }
    }
}
