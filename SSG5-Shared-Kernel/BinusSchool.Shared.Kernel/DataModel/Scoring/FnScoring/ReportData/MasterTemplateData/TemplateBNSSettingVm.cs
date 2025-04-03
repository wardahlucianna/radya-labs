using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportData.MasterTemplateData
{
    public class TemplateBNSSettingVm
    {
        public int PaperSize { set; get; }
        public int Orientation { set; get; }
        public double MarginTop { set; get; }
        public double MarginBottom { set; get; }
        public double MarginLeft { set; get; }
        public double MarginRight { set; get; }
    }

}
