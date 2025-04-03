using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Teaching.FnAssignment.TeacherPositionInfo
{
    public class GetTeacherPositionByUserIDRequest
    {
        public string UserId { get; set; }
        public string IdSchoolAcademicYear { get; set; }
    }
}
