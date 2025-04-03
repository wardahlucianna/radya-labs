using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace BinusSchool.Common.Model.Enums
{
    public enum PaymentStatus
    {
        [Description("Free")]
        Free,

        [Description("Unpaid")]
        Unpaid,

        [Description("Paid")]
        Paid,

        [Description("Payment Expired")]
        Expired,
    }
}
