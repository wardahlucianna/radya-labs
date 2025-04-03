using System;

namespace BinusSchool.Data.Model.Util.FnConverter.StudentAchivementToPdf
{
    public class StudentAchivementToPdfResult
    {
        public string RealFileName { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public Uri Location { get; set; }
    }
}
