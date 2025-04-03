using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSubject.Subject
{
    public class UpdateSubjectRequest : CodeWithIdVm
    {
        public string IdDepartment { get; set; }
        public string IdCurriculumType { get; set; }
        public string IdSubjectType { get; set; }
        public int MaxSession { get; set; }
        public string SubjectId { get; set; }
        public IEnumerable<string> IdPathways { get; set; }
        public IEnumerable<string> IdSubjectLevels { get; set; }
        public IEnumerable<SessionCollectionWithId> Sessions { get; set; }
        public bool IsNeedLessonPlan { get; set; }
    }

    public class SessionCollectionWithId : SessionCollection
    {
        public string Id { get; set; }
    }
}
