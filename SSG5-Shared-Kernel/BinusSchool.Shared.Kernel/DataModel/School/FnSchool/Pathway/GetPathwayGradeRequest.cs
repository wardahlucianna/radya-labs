using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.Pathway
{
    public class GetPathwayGradeRequest : CollectionRequest
    {
        public string IdGrade { get; set; }
    }
}
