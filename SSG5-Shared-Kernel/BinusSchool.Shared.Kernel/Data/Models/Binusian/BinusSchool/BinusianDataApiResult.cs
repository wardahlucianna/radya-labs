namespace BinusSchool.Data.Models.Binusian.BinusSchool
{
    public class BinusianDataApiResult<T> : BinusianApiResult
    {
        public T Data { get; set; }
    }
}
