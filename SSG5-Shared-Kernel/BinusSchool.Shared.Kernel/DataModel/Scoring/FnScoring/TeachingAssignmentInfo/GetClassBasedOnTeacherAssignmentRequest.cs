using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.TeachingAssignmentInfo
{
    public class GetClassBasedOnTeacherAssignmentRequest : GetTeacherAssignmentInfoRequest
    {
        public string IdGrade { get; set; }
        public int Semester { get; set; }
    }
}
