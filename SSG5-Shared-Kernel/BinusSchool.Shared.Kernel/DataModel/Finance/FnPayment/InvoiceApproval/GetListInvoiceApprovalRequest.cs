namespace BinusSchool.Data.Model.Finance.FnPayment.InvoiceApproval
{
    public class GetListInvoiceApprovalRequest
    {
        public string IdAcademicYear { get; set; }
        public int? Semester { get; set; }
        public string? Category { get; set; }
        public string? Status { get; set; }
    }
}
