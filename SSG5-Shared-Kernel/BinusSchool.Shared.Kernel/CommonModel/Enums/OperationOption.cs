using System.ComponentModel;

namespace BinusSchool.Common.Model.Enums
{
    public enum OperationOption
    {
        Null,

        [Description(">=")]
        GreaterThanOrEqual,

        [Description("<=")]
        LessThanOrEqual,

        [Description("<")]
        LessThan,

        [Description(">")]
        GreaterThan,

        [Description("=")]
        Equal,
    }
}
