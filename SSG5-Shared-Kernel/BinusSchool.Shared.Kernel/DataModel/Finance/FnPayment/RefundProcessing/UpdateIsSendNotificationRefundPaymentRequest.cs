using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Finance.FnPayment.RefundProcessing
{
    public class UpdateIsSendNotificationRefundPaymentRequest
    {
        public string IdRefundPayment { get; set; }
        public bool IsSendNotification { get; set; }
    }
}
