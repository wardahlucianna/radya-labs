using System;

namespace BinusSchool.Data.Model.Finance.FnPayment.RefundProcessing
{
    public class UpdateEPRFStatusRequest
    {
        public string IdRefundPayment { get; set; }
        public string EPRFNumber { get; set; }
        public DateTime? RefundDate { get; set; }
    }
}
