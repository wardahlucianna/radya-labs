namespace BinusSchool.Persistence.Abstractions
{
    public interface IActiveState
    {
        bool IsActive { get; set; }
    }
}