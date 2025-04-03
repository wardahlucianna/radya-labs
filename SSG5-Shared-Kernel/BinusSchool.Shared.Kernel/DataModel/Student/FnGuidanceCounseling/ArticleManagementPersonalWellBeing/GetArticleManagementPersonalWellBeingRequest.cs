using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.ArticleManagementPersonalWellBeing
{
    public class GetArticleManagementPersonalWellBeingRequest : CollectionRequest
    {
        public string IdAcademicYear { get; set; }

        public string LevelId { get; set; }

        public string ViewFor { get; set; }

    }
}
