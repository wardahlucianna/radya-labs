using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.ConcernCategory
{
    public class GetConcernCategoryRequest : CollectionRequest
    {
        public string IdSchool { get; set; }
    }
}
