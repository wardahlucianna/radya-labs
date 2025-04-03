using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Util.FnConverter.StudentPassToPdf
{
    public class StudentPassToPdfResult
    {
        public string RealFileName { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public Uri Location { get; set; }
    }
}
