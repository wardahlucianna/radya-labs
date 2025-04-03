using System;

namespace BinusSchool.Data.Model.Finance.FnPayment.RefundProcessing
{
    public class GetListRefundProcessingResult
    {
        public string IdRefundPayment { get; set; }
        public string AcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdReference { get; set; }
        public string RefundName { get; set; }
        //public DateTime? RefundDate { get; set; }
        public int TotalStudent { get; set; }
        //public string StatusEPRF { get; set; }
        //public string EprfNumber { get; set; }
        public DateTime ExpectedDate { get; set; }
        //public bool IsSendNotif { get; set; }
        public string InvoiceStatus { get; set; }
        public string PaymentStatus { get; set; }
        public string IdSupplierInvoice { get; set; }
    }
}
