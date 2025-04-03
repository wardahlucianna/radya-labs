using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Finance.FnPayment.MasterPayment
{
    public class GetPackagePaymentRequest : CollectionRequest
    {
        public string IdSchool { get; set; }
    }
}
