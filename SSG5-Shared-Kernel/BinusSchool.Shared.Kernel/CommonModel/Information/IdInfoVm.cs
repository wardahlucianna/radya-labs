namespace BinusSchool.Common.Model.Information
{
    public class IdInfoVm : IdSiblingInfoVm
    {
        public string IdRegistrant { get; set; }
        public string IdSchool { get; set; }
        public string IdBinusian { get; set; }
        public string NISN { get; set; }
    }
    public class IdSiblingInfoVm
    {
        public string IdSiblingGroup { get; set; }
    }
}
