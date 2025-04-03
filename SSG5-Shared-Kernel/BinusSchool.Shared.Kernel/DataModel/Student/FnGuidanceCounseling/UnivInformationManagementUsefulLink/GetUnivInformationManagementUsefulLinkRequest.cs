using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.UnivInformationManagementUsefulLink
{
    public class GetUnivInformationManagementUsefulLinkRequest : CollectionRequest
    {
        public string IdAcademicYear { get; set; }

        public string GradeId { get; set; }
    }
}
