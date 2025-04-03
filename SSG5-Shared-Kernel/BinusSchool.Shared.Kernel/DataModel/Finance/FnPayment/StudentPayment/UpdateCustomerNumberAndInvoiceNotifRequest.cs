using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Finance.FnPayment.StudentPayment
{
    public class UpdateCustomerNumberAndInvoiceNotifRequest
    {
        public bool SendInvoiceApprovalNotification { get; set; }
        public List<UpdateCustomerNumberAndInvoiceNotifRequest_StudentInvoice> StudentInvoiceDataList { get; set; }
    }

    public class UpdateCustomerNumberAndInvoiceNotifRequest_StudentInvoice
    {
        public string IdTransaction { get; set; }
        //public bool UpdateCustomerNumber { get; set; }
        public bool SendInvoiceNotification { get; set; }
    }
}
