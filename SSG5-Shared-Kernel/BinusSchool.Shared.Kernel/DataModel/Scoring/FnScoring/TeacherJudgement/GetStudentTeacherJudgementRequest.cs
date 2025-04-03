using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.TeacherJudgement
{
    public class GetStudentTeacherJudgementRequest
    {
        public string IdUser { set; get; }
        public string IdLesson { set; get; }
        public string IdSubject { set; get; }
        public string IdSubjectLevel { set; get; }
        public string IdPeriod { set; get; }
        public string IdSubComponent { set; get; }
    }
}

