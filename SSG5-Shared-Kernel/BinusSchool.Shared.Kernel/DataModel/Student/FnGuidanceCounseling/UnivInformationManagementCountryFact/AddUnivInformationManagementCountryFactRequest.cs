using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.UnivInformationManagementCountryFact
{
    public class AddUnivInformationManagementCountryFactRequest 
    {
        public string IdAcademicYear { get; set; }
        public List<LevelUnivInformationManagementCountryFact> LevelIds { get; set; }
        public string CountryName { get; set; }
        public string CountryDescription { get; set; }
        public string CountryWebsite { get; set; }
        public string ContactPerson { get; set; }
        public List<FactSheetUnivInformationManagementCountryFact> FactSheet { get; set; }
        public List<LogoUnivInformationManagementCountryFact> Logo { get; set; }
    }

    public class LevelUnivInformationManagementCountryFact
    {
        public string Id { get; set; }
    }

    public class FactSheetUnivInformationManagementCountryFact
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public string OriginalFilename { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public int FileSize { get; set; }
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
