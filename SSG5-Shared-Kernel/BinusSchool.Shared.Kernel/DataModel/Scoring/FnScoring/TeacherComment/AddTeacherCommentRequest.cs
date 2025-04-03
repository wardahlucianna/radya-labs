using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.TeacherComment
{
    public class AddTeacherCommentRequest
    {
        public string IdPeriod { get; set; }
        public string IdStudent { get; set; }
        public string IdSubject { get; set; }
        public string Comment { get; set; }
        public string UserIn { get; set; }
    }
}
