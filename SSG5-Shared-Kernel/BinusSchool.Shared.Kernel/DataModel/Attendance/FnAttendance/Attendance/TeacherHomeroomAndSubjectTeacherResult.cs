using System.Collections.Generic;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Attendance.FnAttendance.TeacherHomeroomAndSubjectTeacher
{
    public class TeacherHomeroomAndSubjectTeacherResult
    {
        public List<AbsentTerm> HomeroomTerms { get; set; }
        public List<AbsentTerm> SubjectTeacherTerms { get; set; }
        public bool IsNeedValidation { get; set; }
    }
}
