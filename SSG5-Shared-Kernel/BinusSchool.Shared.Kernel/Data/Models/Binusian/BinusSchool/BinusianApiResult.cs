namespace BinusSchool.Data.Models.Binusian.BinusSchool
{
    public abstract class BinusianApiResult
    {
        public int ResultCode { get; set; }
        public string ErrorMessage { get; set; }
    }
}
