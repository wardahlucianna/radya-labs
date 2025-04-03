using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnMovingStudent.MoveStudentEnrollment
{
    public class GetMoveStudentEnrollmentHistoryRequest : CollectionSchoolRequest
    {
        public string idHomeroomStudent { get; set; }
    }
}
