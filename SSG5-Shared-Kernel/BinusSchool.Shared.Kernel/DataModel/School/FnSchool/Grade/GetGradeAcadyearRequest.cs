using System;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.Grade
{
    public class GetGradeAcadyearRequest : CollectionRequest
    {
        public string IdAcadyear { get; set; }
        public bool? ExcludeHavePeriod { get; set; }
        public bool? ExcludeHaveSubject { get; set; }
        public bool? ExcludeHavePathway { get; set; }
    }
}
