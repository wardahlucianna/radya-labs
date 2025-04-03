using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.StudentEnrollmentScore
{
    public class GetLessonTakenByStudentDetailForSummaryRequest
    {
        public string IdStudent { get; set; }
        public string IdAcademicYear { get; set; }
    }
}
