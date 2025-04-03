using System;

namespace BinusSchool.Data.Model.Finance.FnPayment.InvoiceApproval
{
    public class GetStudentInvoiceApprovalDetailResult
    {
        public int ApproverLevel { get; set; }
        public string BinusianID { get; set; }
        public string ApproverName { get; set; }
        public bool? ApprovalStatus { get; set; }
        public DateTime? ApprovalDate { get; set; }
    }
}
