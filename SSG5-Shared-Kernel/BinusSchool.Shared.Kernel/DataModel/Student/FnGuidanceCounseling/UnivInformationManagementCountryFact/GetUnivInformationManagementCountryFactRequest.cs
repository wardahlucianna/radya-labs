using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.UnivInformationManagementCountryFact
{
    public class GetUnivInformationManagementCountryFactRequest : CollectionRequest
    {
        public string IdAcademicYear { get; set; }

        public string LevelId { get; set; }
    }
}
