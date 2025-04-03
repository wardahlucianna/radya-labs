using System;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Finance.FnPayment.RefundProcessing
{
    public class GetDetailRefundProcessingResult
    {
        public ItemValueVm AcademicYear { get; set; }
        public ItemValueVm Semester { get; set; }
        public string RefundName { get; set; }
        public DateTime ExpectedDate { get; set; }
        public ItemValueVm CostCenter { get; set; }
        public ItemValueVm Project { get; set; }
    }
}
