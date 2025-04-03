namespace BinusSchool.Common.Model.Abstractions
{
    public interface IPagination
    {
        int Page { get; set; }
        int Size { get; set; }
        int CalculateOffset();
    }
}