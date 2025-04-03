using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.ArticleManagementGcLink
{
    public class AddArticleManagementGcLinkRequest
    {
        public string IdAcademicYear { get; set; }

        public List<GradeArticleManagementGcLink> GradeIds { get; set; }

        public string LinkDescription { get; set; }

        public string Link { get; set; }

        public List<LogoArticleManagementGcLink> Logo { get; set; }
    }

    public class GradeArticleManagementGcLink
    {
        public string Id { get; set; }
    }

    public class LogoArticleManagementGcLink
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public string OriginalFilename { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public int FileSize { get; set; }
    }
}
