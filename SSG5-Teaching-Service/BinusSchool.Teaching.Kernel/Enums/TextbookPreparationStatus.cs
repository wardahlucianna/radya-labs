﻿using System.ComponentModel;

namespace BinusSchool.Teaching.Kernel.Enums
{
    public enum TextbookPreparationStatus
    {
        Hold,
        [Description("On Review (1)")]
        OnReview1,
        [Description("On Review (2)")]
        OnReview2,
        [Description("On Review (3)")]
        OnReview3,
        Declined,
        Approved
    }
}
