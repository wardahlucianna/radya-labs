using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSubject.Subject
{
    public class GetSubjectDetailResult : DetailResult2
    {
        public CodeWithIdVm Acadyear { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public CodeWithIdVm Department { get; set; }
        public CodeWithIdVm CurriculumType { get; set; }
        public CodeWithIdVm SubjectType { get; set; }
        public IEnumerable<CodeWithIdVm> Pathways { get; set; }
        public IEnumerable<CodeWithIdVm> SubjectLevels { get; set; }
        public string SubjectId { get; set; }
        public int MaxSession { get; set; }
        public IEnumerable<SessionCollectionWithId> Sessions { get; set; }
        public bool IsNeedLessonPlan { get; set; }
    }
}
