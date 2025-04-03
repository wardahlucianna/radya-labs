using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.UnivInformationManagementUsefulLink
{
    public class AddUnivInformationManagementUsefulLinkRequest
    {
        public string IdAcademicYear { get; set; }

        public List<GradeUnivInformationManagementUsefulLink> GradeIds { get; set; }

        public string LinkDescription { get; set; }

        public string Link { get; set; }

        //public List<LogoUnivInformationManagementUsefulLink> Logo { get; set; }

    }

    public class GradeUnivInformationManagementUsefulLink
    {
        public string Id { get; set; }
    }

    //public class LogoUnivInformationManagementUsefulLink
    //{
    //    public string Id { get; set; }
    //    public string Url { get; set; }
    //    public string OriginalFilename { get; set; }
    //    public string FileName { get; set; }
    //    public string FileType { get; set; }
    //    public decimal FileSize { get; set; }
    //}
}
