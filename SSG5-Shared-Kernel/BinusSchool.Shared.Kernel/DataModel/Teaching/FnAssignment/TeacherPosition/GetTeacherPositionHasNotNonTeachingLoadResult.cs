using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Teaching.FnAssignment.TeacherPosition
{
    public class GetTeacherPositionHasNotNonTeachingLoadResult : CodeWithIdVm
    {
        public string IdSchool { get; set; }
        public string Alias { get; set; }
        public string Position { get; set; }
    }
}
