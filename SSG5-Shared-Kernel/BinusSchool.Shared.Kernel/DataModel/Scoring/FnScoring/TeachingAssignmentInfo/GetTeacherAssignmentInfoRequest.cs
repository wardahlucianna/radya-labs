using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.TeachingAssignmentInfo
{
    public class GetTeacherAssignmentInfoRequest
    {
        public string IdUser { get; set; }
        public string IdAcademicYear { get; set; }
        public string Search { get; set; }
    }
}
