using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Teaching.FnAssignment.TeacherPosition
{
    public class GetTeacherPositionResult : CodeWithIdVm
    {
        public string IdSchool { get; set; }
        public ICollection<GetTeacherPositionAliasResult> TeacherPositionAlias { get; set; }
        public string Position { get; set; }
        public AcademicType Category { get; set; }
        public bool CanDeleted { get; set; }
    }
    public class GetTeacherPositionAliasResult
    {
        public string Id { get; set; }
        public string IdTeacherPosition { get; set; }
        public GetTeacherPositionAliasLevelResult Level { get; set; }
        public string Alias { get; set; }
    }

    public class GetTeacherPositionAliasLevelResult : CodeWithIdVm
    {
        public CodeWithIdVm Academicyear { get; set; }
    }
}
