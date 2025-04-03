using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.ArticleManagementPersonalWellBeing
{
    public class UpdateArticleManagementPersonalWellBeingRequest
    {
        public string IdArticleManagementPersonalWellBeing { get; set; }
        public List<LevelArticleManagementPersonalWellBeing> LevelIds { get; set; }
        public string ArticleName { get; set; }
        public string ArticleDescription { get; set; }
        public string Link { get; set; }
        public PersonalWellBeingFor ViewFor { get; set; }
        public List<AttachmentArticleManagementPersonalWellBeing> Attachments { get; set; }
        public bool NotifyRecipient { get; set; }
    }
}
