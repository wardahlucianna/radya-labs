using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.TeachingAssignmentInfo
{
    public class GetClassroomBasedOnTeacherAssignmentRequest : GetTeacherAssignmentInfoRequest
    {
        public string IdGrade { get; set; }
    }
}
