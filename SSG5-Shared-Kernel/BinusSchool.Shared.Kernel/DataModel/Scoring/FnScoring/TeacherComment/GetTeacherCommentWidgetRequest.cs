using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.TeacherComment
{
    public class GetTeacherCommentWidgetRequest
    {
        public string UserId { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdPeriod { get; set; }
        public string RoleCode { get; set; }
        public string IdGrade { get; set; }
        public string SubjectClassroomId { get; set; }
        public string IdSubject { get; set; }
        public string IdStudent { get; set; }
    }
}
