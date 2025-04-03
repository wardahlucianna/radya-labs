﻿using System.ComponentModel;

namespace BinusSchool.School.Kernel.Enums
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
