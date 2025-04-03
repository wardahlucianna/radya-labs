using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.TeacherPrivilege
{
    public class GetTeacherPrivilageForTeacherCommentResult
    {
        public List<TCResult_Grade>? Grades { get; set; }
    }
    public class TCResult_Grade : ItemValueVm
    {
        public List<TCResult_TypeVM_Term>? HomeroomTeacher { get; set; }
        public List<TCResult_TypeVM_Term>? SubjectTeacher { get; set; }
    }
    public class TCResult_TypeVM_Term : ItemValueVm
    {
        public List<TCResult_TypeVM_Class>? ClassIdHomeroom { get; set; }
        public int? Semester { get; set; }
    }
    public class TCResult_TypeVM_Class : ItemValueVm
    {
        public string IdSubject { get; set; }
        public List<ItemValueVm>? Students { get; set; }
    }
}
