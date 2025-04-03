using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Teaching.FnAssignment.TeacherAssignment
{
    public class AddTeacherAssignmentRequest
    {
        public string IdAcademicYear { get; set; }
        public List<NonteacingLoadAcademic> NonteacingLoadAcademic { get; set; }
        public List<NonteacingLoadNonAcademic> NonteacingLoadNonAcademic { get; set; }

    }

    public class NonteacingLoadAcademic
    {
        public string Id { get; set; }
        public string IdSchoolUser { get; set; }
        public string IdSchoolNonTeachingLoad { get; set; }
        public int Load { get; set; }
        public string Data { get; set; }
    }

    public class NonteacingLoadNonAcademic
    {
        public string Id { get; set; }
        public string IdSchoolUser { get; set; }
        public string IdSchoolNonTeachingLoad { get; set; }
        public int Load { get; set; }
        public string Data { get; set; }
    }
}
