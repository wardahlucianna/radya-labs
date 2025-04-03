using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.ArticleManagementGcLink
{
    public class GetArticleManagementGcLinkResult : ItemValueVm
    {
        public CodeWithIdVm AcademicYear { get; set; }

        public List<CodeWithIdVm> Grade { get; set; }

        public string Link { get; set; }

        public List<LogoArticleManagementGcLink> Logo { get; set; }
    }
}
