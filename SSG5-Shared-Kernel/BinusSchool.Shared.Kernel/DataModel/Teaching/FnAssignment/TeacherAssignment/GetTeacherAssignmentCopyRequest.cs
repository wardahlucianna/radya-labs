using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Teaching.FnAssignment.TeacherAssignment
{
    public class GetTeacherAssignmentCopyRequest
    {
        public string IdUser { get; set; }
        public string IdAcademicYearFrom { get; set; }
        public string IdAcademicYearTo { get; set; }
        public AcademicType Category { get; set; }
    }
}
