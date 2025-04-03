using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Teaching.FnAssignment.MaxTeacherLoad
{
    public class GetTeacherMaxLoadDetailResult : DetailResult2
    {
        public string IdAcademicYear { get; set; }
        public int MaxLoad { get; set; }
    }
}
