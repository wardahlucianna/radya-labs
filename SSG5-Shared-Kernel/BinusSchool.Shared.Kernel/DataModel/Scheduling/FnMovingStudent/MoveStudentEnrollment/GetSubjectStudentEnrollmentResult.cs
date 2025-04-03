using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnMovingStudent.MoveStudentEnrollment
{
    public class GetSubjectStudentEnrollmentResult
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string IdLesson { get; set; }
        public List<ItemValueVm> subjectLevel { get; set; }
    }
}
