using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Teaching.FnAssignment.TeacherAssignment
{
    public class GetTeacherAssignmentCopyResult
    {
        public List<NonTeacingLoadFromAndTo> NonTeachingAssignmentAcademic { get; set; }
        public List<NonTeacingLoadFromAndTo> NonTeachingAssignmentNonAcademic { get; set; }
        public List<ListNonTeachingAcademic> ListNonTeachingAcademics { get; set; }
        public List<ListNonTeachingAcademic> ListNonTeachingNonAcademics { get; set; }
    }

    public class NonTeacingLoadFromAndTo
    {
        public AcademicType Category { get; set; }
        public bool IsDisabled { get; set; }
        public List<string> MasterError { get; set; }
        public NonTeacingLoad NonTeachingLoadFrom { get; set; }
        public NonTeacingLoad NonTeachingLoadTo { get; set; }
    }

    public class NonTeacingLoad
    {
        public string Id { get; set; }
        public string IdUser { get; set; }
        public string IdMsNonTeachingLoad { get; set; }
        public int Load { get; set; }
        public string Data { get; set; }
    }
}
