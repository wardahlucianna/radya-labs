using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.GcCorner
{
    public class GetGcCornerCountryFactResult : ItemValueVm
    {
        public string CountryName { get; set; }
        public string CountryWebsite { get; set; }
        public string ContactPerson { get; set; }

        public List<FactSheetUnivInformationManagementCountryFact> FactSheet { get; set; }

        public List<LogoUnivInformationManagementCountryFact> Logo { get; set; }
    }

    public class FactSheetUnivInformationManagementCountryFact
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public string OriginalFilename { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public decimal FileSize { get; set; }
    }

    public class LogoUnivInformationManagementCountryFact
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public string OriginalFilename { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public int FileSize { get; set; }
    }
}
