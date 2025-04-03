using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.TeacherComment
{
    public class GetStudentScoreDetailRequest
    {
        public string StudentId { get; set; }
        public string IdGrade { get; set; }
        public int Semester { get; set; }
    }
}
