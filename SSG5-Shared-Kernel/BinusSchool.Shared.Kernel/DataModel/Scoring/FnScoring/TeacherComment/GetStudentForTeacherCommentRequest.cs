using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.TeacherComment
{
    public class GetStudentForTeacherCommentRequest
    {
        public string IdGrade { get; set; }
        public string IdClassroom { get; set; }
        public int SmtId { get; set; }
        public string Search { get; set; }
    }
}
