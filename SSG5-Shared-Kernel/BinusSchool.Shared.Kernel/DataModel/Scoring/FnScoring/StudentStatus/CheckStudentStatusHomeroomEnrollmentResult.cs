using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.StudentStatus
{
    public class CheckStudentStatusHomeroomEnrollmentResult
    {
        public string IdStudent { get; set; }
        public string StudentName { get; set; }
        public string IdHomeroom { get; set; }
        public string HomeroomName { get; set; }
        public string IdGrade { get; set; }
        public string GradeName { get; set; }
        public string IdSubject { get; set; }
        public string SubjectName { get; set; }
        public string IdSubjectLevel { get; set; }
        public string IdLesson { get; set; }
        public ItemValueVm StudentStatus { get; set; }
        public bool IsExitStudent { get; set; }
    }
}
