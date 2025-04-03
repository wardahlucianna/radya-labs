namespace BinusSchool.Common.Model.Abstractions
{
    public interface IItemValueVm : IUniqueId
    {
        string Description { get; set; }
    }
}