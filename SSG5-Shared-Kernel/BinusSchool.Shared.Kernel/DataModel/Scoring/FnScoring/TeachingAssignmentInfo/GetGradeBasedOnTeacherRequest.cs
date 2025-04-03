using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.TeachingAssignmentInfo
{
    public class GetGradeBasedOnTeacherRequest : GetTeacherAssignmentInfoRequest
    {
        public string IdLevel { get; set; }
    }
}
