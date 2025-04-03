using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Teaching.FnAssignment.TeacherPosition
{
    public class GetTeacherPositionDetailResult : DetailResult2
    {
        public string IdSchool { get; set; }
        public CodeWithIdVm Position { get; set; }
        public AcademicType Category { get; set; }
        public ICollection<GetTeacherPositionAliasResult> TeacherPositionAlias { get; set; }
    }
}
