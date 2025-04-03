using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Finance.FnPayment.Queue
{
    /*
     * CAUTION:
     * PLEASE BE CAREFUL WHEN MODIFY THIS FUNCTION BECAUSE THIS FUNCTION IS USED BY VIRTUAL ACCOUNT APPLICATION TOO!
     */

    public class SendNotificationPaidInvoiceToParentQueueRequest
    {
        public string IdSchool { get; set; }
        public string IdStudent { get; set; }
        public string PaymentReferenceNumber { get; set; }
    }
}
