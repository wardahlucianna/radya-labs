using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Teaching.FnAssignment.NonTeachingLoad
{
    public class GetNonTeachLoadRequest : CollectionSchoolRequest
    {
        public string IdAcadyear { get; set; }
        public AcademicType? Category { get; set; }
    }
}
