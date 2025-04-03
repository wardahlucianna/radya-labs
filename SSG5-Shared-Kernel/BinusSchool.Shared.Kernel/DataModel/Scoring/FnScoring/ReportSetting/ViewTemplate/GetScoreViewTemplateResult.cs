using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportSetting.ViewTemplate
{
    public class GetScoreViewTemplateResult : CodeWithIdVm
    {       
        public string ShortDesc { set; get; }
        public string LongDesc { set; get; }      
        public bool CurrentStatus { set; get; }
        public int ActiveStatus { set; get; }
        public bool ShowDeleteButton { set; get; }
        public bool ShowEditButton { set; get; }
        public string Template { set; get; }
    }
}
