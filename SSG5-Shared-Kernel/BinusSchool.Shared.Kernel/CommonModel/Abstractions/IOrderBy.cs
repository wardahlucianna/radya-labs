using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Common.Model.Abstractions
{
    public interface IOrderBy
    {
        string OrderBy { get; set; }
        OrderType OrderType { get; set; }
    }
}