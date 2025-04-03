using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSubject.Subject
{
    public class AddSubjectRequest : CodeVm
    {
        public string IdAcadyear { get; set; }
        public string IdDepartment { get; set; }
        public string IdCurriculumType { get; set; }
        public string IdSubjectType { get; set; }
        public IEnumerable<AddGradePathway> Grades { get; set; }
        public int MaxSession { get; set; }
        public IEnumerable<SessionCollection> Sessions { get; set; }
        public bool IsNeedLessonPlan { get; set; }
    }

    public class AddGradePathway
    {
        public string IdGrade { get; set; }
        public IEnumerable<string> IdPathways { get; set; }
        public IEnumerable<string> IdSubjectLevels { get; set; }
        public string SubjectId { get; set; }
    }

    public class SessionCollection
    {
        public int Length { get; set; }
        public int Count { get; set; }
    }
}
