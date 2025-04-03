using System;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.Grade
{
    public class GetGradeWithPathwayRequest : CollectionRequest
    {
        public string IdAcadyear { get; set; }
        public string IdSessionSet { get; set; }
    }
}
