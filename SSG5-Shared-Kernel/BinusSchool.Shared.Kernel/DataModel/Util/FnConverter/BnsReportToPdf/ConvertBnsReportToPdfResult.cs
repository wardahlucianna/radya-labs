using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Util.FnConverter.BnsReportToPdf
{
    public class ConvertBnsReportToPdfResult
    {
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public string Location { get; set; }
        public string HtmlBody { get; set; }
        public string HtmlHeader { get; set; }
        public string HtmlFooter { get; set; }
        public List<string> GenerateStatus { get; set; }

    }
}
