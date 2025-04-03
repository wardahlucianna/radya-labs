using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Finance.FnPayment.Invoice
{
    public class GetSummaryInvoiceResult
    {

        public string IdEventPayment { set; get; }
        public string EventPaymentName { set; get; }     
        public string AcademicYear { set; get; }
        public string Semester  { set; get; }
        public DateTime PaymentStartDateTime  { set; get; }
        public DateTime PaymentEndDateTime { set; get; }
        public string PackageName { set; get; }
        public string Description { set; get; }
        public int TotalStudent { set; get; }
        public decimal TotalInvoice { set; get; }
        public int TotalUnpaid { set; get; }
        public string Status { set; get; }
        public DateTime? DateIn { set; get; }
        public DateTime? DateUp { set; get; }

    }
}
