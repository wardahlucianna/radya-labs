using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Finance.FnPayment.InvoiceApproval
{
    public class GetListStudentInvoiceResult
    {
        public GetStudentInvoiceResult_Header Header { get; set; }
        public List<GetStudentInvoiceResult_Body> Data { get; set; }
    }

    public class GetStudentInvoiceResult_Header
    {
        public string IdEventPayment { get; set; }
        public ItemValueVm AcademicYear { get; set; }
        public int Semester { get; set; }
        public string PaymentName { get; set; }
        public string PaymentDescription { get; set; }
        public string PaymentPeriod { get; set; }
        public List<NameValueVm> PIC { get; set; }
        public List<NameValueVm> Approver { get; set; }
        public GetStudentInvoiceResult_UserDetail Requester { get; set; }
        public GetStudentInvoiceResult_UserDetail ApproverNext { get; set; }
        public GetStudentInvoiceResult_UserDetail ApproverLast { get; set; }
        public string ApprovalRemarks { get; set; }
    }

    public class GetStudentInvoiceResult_Body
    {
        public NameValueVm Student { get; set; }
        public string Level { get; set; }
        public string Grade { get; set; }
        public string Homeroom { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class GetStudentInvoiceResult_UserDetail
    {
        public NameValueVm User { get; set; }
        public string Email { get; set; }
    }
}
