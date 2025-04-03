using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.PrivilegeEntryScore
{
    public class GetPrivilegeEntryScoreByPositionNewResult
    {
        public string Semester { set; get; }
        public string IdTerm { set; get; }
        public string TermName { set; get; }
        public string IdGrade { set; get; }
        public string GradeName { set; get; }
        public int GradeOrder { set; get; }
        public bool IsSubjectCLA { set; get; }
        public string Subject { set; get; }
        public string SubjectName { set; get; }
        public string LessonPathway { set; get; }
        public string LessonPathwayName { set; get; }
        public string Lesson { set; get; }
        public string LessonName { set; get; }
    }
}
