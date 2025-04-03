using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Teaching.FnAssignment.TeacherByAssignment
{
    public class GetTeacherByGradeRequest
    {
        public string AcademicYearId { get; set; }
        public string Grade { get; set; }
    }
}
