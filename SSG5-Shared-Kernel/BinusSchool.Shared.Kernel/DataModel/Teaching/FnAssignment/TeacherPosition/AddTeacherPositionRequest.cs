using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Teaching.FnAssignment.TeacherPosition
{
    public class AddTeacherPositionRequest : CodeVm
    {
        public string IdSchool { get; set; }

        public string IdPosition {get;set;}
        public object Alias { get; set; }
        public AcademicType Category { get; set; }
        public ICollection<AddTeacherPositionAliasRequest> AliasLevel { get; set; }
    }

    public class AddTeacherPositionAliasRequest
    {
        public string Id { get; set; }
        public string IdTeacherPosition { get; set; }
        public string IdLevel { get; set; }
        public string Alias { get; set; }
    }
}
