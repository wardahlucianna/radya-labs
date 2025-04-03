using System;

namespace BinusSchool.Data.Model.Finance.FnPayment.RefundProcessing
{
    public class AddRefundProcessingRequest
    {
        public string? IdRefundPayment { get; set; }
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string RefundName { get; set; }
        public DateTime ExpectedDate { get; set; }
        public string IdCostCenter { get; set; }
        public string IdProject { get; set; }
    }
}
