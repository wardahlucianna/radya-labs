using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace BinusSchool.Common.Model.Enums
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
