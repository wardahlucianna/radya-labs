using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Teaching.FnAssignment.NonTeachingLoad
{
    public class GetCopyNonTeachingLoadRequest : CollectionSchoolRequest
    {
        public string IdAcadYearTarget { get; set; }
        public string IdAcadYearSource { get; set; }
        public AcademicType? Category { get; set; }
    }
}
