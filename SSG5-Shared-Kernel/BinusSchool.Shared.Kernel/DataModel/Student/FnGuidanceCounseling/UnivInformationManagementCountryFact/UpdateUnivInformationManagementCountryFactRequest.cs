using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.UnivInformationManagementCountryFact
{
    public class UpdateUnivInformationManagementCountryFactRequest
    {
        public string IdUnivInformationManagementCountryFact { get; set; }
        public List<LevelUnivInformationManagementCountryFact> LevelIds { get; set; }
        public string CountryName { get; set; }
        public string CountryDescription { get; set; }
        public string CountryWebsite { get; set; }
        public string ContactPerson { get; set; }
        public List<FactSheetUnivInformationManagementCountryFact> FactSheet { get; set; }
        public List<LogoUnivInformationManagementCountryFact> Logo { get; set; }
    }
}
