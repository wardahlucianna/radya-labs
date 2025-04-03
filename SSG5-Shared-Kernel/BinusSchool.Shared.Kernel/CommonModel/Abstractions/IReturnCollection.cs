using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Common.Model.Abstractions
{
    public interface IReturnCollection
    {
        CollectionType Return { get; set; }
    }
}