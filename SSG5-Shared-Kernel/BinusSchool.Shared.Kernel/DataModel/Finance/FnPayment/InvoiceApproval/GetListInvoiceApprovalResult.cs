using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Finance.FnPayment.InvoiceApproval
{
    public class GetListInvoiceApprovalResult
    {
        public string IdEventPayment { get; set; }
        public string AcademicYear { get; set; }
        public int Semester { get; set; }
        public string PaymentName { get; set; }
        public string Category { get; set; }
        public decimal TotalInvoice { get; set; }
        public List<string> PIC { get; set; }
        public ItemValueVm Status { get; set; }
        public GetListInvoiceApprovalResult_Approver UserLogin { get; set; }
        public GetListInvoiceApprovalResult_Approver PrevApprover { get; set; }
        public GetListInvoiceApprovalResult_Approver CurrentApprover { get; set; }
        public GetListInvoiceApprovalResult_Approver NextApprover { get; set; }
        public GetListInvoiceApprovalResult_Approver LastApprover { get; set; }
        public bool IsApprover { get; set; }
        public bool IsPIC { get; set; }
        public bool IsShowButton { get; set; }
    }

    public class GetListInvoiceApprovalResult_Approver
    {
        public string Approver { get; set; }
        public int ApproverLevel { get; set; }
    }
}
