using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.StudentExtracurricular
{
    public class UpdateStudentExtracurricularPriorityRequest
    {
        public string IdGrade { get; set; }
        public int Semester { get; set; }
        public string IdStudent { get; set; }
        public List<UpdateStudentExtracurricularRequest_ExtracurricularPrimary> ExtracurricularPrimaryList { get; set; }
    }

    public class UpdateStudentExtracurricularRequest_ExtracurricularPrimary
    {
        public string IdExtracurricular { get; set; }
        public bool IsPrimary { get; set; }
    }
}
