using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ProgressStatus
{
    public class GetProgressStatusHomeroomResult
    {
        public ItemValueVm Homeroom;
        public NameValueVm Student { get; set; }
        public string ProgressStatus { get; set; }
        public bool? SubmitAgreement { get; set; }
        public string EnglishVersion { get; set; }
        public string IndonesiaVersion { get; set; }
        public string NationalStatus { get; set; }
        public bool? HideReportCard { get; set; }
    }
}
