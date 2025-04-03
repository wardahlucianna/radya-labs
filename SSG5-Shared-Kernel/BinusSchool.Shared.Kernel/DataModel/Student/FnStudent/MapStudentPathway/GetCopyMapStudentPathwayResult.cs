using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.MapStudentPathway
{
    public class GetCopyMapStudentPathwayResult : GetMapStudentPathwayResult
    {
        public bool IsShowWarning { get; set; }
        public string IdStudentGradeNextAy { get; set; }
        public string IdStudentGradeCurrentAy { get; set; }
    }
}
