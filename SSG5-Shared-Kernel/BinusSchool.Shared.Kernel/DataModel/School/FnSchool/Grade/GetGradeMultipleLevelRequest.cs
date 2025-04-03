using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.Grade
{
    public class GetGradeMultipleLevelRequest
    {
        public string IdSchool { get; set; }
        public string IdAcadyear { get; set; }
        public List<string> IdLevel { get; set; }
    }
}
