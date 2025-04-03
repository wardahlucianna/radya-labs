using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Finance.FnPayment.Invoice
{
    public class GetMasterInvoiceDetailRequest
    {
        public GetMasterInvoiceDetailRequest()
        {
            packageName = "";
        }
        public string IdEventPayment { set; get; }
        public string packageName { set; get; }
    }
}
