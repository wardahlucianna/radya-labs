using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Teaching.FnAssignment.TeacherByAssignment
{
    public class GetTeacherByDepartmentRequest
    {
        public string Department { get; set; }
        public string AcademicYearId { get; set; }
    }
}
