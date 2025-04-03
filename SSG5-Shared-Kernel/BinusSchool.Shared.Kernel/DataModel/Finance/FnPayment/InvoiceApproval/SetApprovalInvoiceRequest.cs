using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Finance.FnPayment.InvoiceApproval
{
    public class SetApprovalInvoiceRequest
    {
        public string IdEventPayment { get; set; }
        public bool ApprovalStatus { get; set; }
        public string IdBinusian { get; set; }
        public string Remarks { get; set; }
    }
}
