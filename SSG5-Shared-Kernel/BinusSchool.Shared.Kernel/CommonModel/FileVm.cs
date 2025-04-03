namespace BinusSchool.Common.Model
{
    public class FileVm
    {
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public byte[] FileContents { get; set; }
    }
}