using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.ArticleManagementGcLink
{
    public class UpdateArticleManagementGcLinkRequest
    {
        public string IdArticleManagementPersonalGcLink { get; set; }
        public List<GradeArticleManagementGcLink> GradeIds { get; set; }
        public string LinkDescription { get; set; }
        public string Link { get; set; }
        public List<LogoArticleManagementGcLink> Logo { get; set; }
    }
}
