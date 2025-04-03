using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Finance.FnPayment.Invoice
{
    public class UpdateMasterInvoiceRequest
    {
        public bool invoiceType { set; get; }
        public string IdEventPayment { set; get; }
        public string paymentName { set; get; }
        public string description { set; get; }
        public DateTime paymentStartDateTime { set; get; }
        public DateTime paymentEndDateTime { set; get; }
        public List<string> PIC { set; get; }
        public bool NeedApproval { set; get; }
        public List<ApproverInvoicePaymentVm> approver { set; get; }
    }
}
