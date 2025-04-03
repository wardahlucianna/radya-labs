using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.UnivInformationManagementUsefulLink
{
    public class UpdateUnivInformationManagementUsefulLinkRequest
    {
        public string IdUnivInformationManagementUsefulLink { get; set; }
        public List<GradeUnivInformationManagementUsefulLink> GradeIds { get; set; }
        public string LinkDescription { get; set; }
        public string Link { get; set; }
        //public List<LogoUnivInformationManagementUsefulLink> Logo { get; set; }
    }
}
