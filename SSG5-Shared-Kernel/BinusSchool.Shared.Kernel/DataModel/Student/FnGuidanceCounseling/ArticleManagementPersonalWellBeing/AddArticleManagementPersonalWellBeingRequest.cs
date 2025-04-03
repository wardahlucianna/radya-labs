using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.ArticleManagementPersonalWellBeing
{
    public class AddArticleManagementPersonalWellBeingRequest
    {
        public string IdAcademicYear { get; set; }
        public List<LevelArticleManagementPersonalWellBeing> LevelIds { get; set; }
        public string ArticleName { get; set; }
        public PersonalWellBeingFor ViewFor { get; set; }
        public string ArticleDescription { get; set; }
        public string Link { get; set; }
        public List<AttachmentArticleManagementPersonalWellBeing> Attachments { get; set; }
        public bool NotifyRecipient { get; set; }
    }

    public class LevelArticleManagementPersonalWellBeing
    {
        public string Id { get; set; }
    }

    public class AttachmentArticleManagementPersonalWellBeing
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public string OriginalFilename { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public int FileSize { get; set; }
    }
}
