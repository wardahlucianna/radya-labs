using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Teaching.FnAssignment.Timetable
{
    public class GetTimeTableByUserRequest
    {
        public string IdSchoolUser { get; set; }
        public string IdSchoolAcademicYear { get; set; }
    }
}
