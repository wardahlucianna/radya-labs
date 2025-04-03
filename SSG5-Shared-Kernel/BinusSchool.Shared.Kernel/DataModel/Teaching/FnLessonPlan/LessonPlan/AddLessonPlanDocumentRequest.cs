using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinusSchool.Data.Model.Teaching.FnLessonPlan.LessonPlan
{
    public class AddLessonPlanDocumentRequest
    {
        public string PathFile { get; set; }
        public string Filename { get; set; }
        // public string IdLessonPlan { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdGrade { get; set; }
        public int WeekNumber { get; set; }
        public string IdPeriod { get; set; }
        public List<SubjectLessonPlanDocument> Subject { get; set; }
        public bool IsAllSubject { get; set; }
        public string IdUser { get; set; }
    }

    public class SubjectLessonPlanDocument
    {
        public string IdSubject { get; set; }
        public string IdSubjectMappingSubjectLevel { get; set; }

    }
}
