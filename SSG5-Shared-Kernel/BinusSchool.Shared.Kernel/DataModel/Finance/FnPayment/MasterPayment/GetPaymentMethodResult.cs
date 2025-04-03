using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Finance.FnPayment.MasterPayment
{
    public class GetPaymentMethodResult
    {
        public string IdPaymentMethod {  get; set; }
        public string PaymentMethodName { get; set; }
        public int? OrderNo { get; set; }
    }
}
