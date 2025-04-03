using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.ArticleManagementPersonalWellBeing
{
    public class GetArticleManagementPersonalWellBeingResult : ItemValueVm
    {

        public CodeWithIdVm AcademicYear { get; set; }

        public List<CodeWithIdVm> Level { get; set; }

        public string ArticleName { get; set; }

        public string ViewFor { get; set; }

        public string Link { get; set; }

        public List<AttachmentArticleManagementPersonalWellBeing> Attachments { get; set; }

        public bool NotifyRecipient { get; set; }
    }
}
