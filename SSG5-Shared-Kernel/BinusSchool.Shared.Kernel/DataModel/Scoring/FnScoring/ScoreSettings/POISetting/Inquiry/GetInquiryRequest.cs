using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.POISetting.Inquiry
{
    public class GetInquiryRequest : CollectionRequest
    {
        public string IdSchool { get; set; }
    }
}
