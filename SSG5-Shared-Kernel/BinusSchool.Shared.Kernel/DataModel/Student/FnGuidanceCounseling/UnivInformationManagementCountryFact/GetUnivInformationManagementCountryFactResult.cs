using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.UnivInformationManagementCountryFact
{
    public class GetUnivInformationManagementCountryFactResult : ItemValueVm
    {
        public CodeWithIdVm AcademicYear { get; set; }

        public List<CodeWithIdVm> Level { get; set; }

        public string CountryName { get; set; }
        public string CountryWebsite { get; set; }
        public string ContactPerson { get; set; }

        public List<FactSheetUnivInformationManagementCountryFact> FactSheet { get; set; }

        public List<LogoUnivInformationManagementCountryFact> Logo { get; set; }
    }
}
