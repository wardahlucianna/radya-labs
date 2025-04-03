using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.StudentEnrollmentScore
{
    public class GetLessonTakenByStudentDetailForReportResult
    {
        public string IdHomeroom { set; get; }
        public string IdLesson { set; get; }
        public string IdSubjectScoreFinalSetting { set; get; }
        public string ClassIdGenerated { set; get; }
        public string IdSubject { set; get; }
        public string SubjectName { set; get; }
        public string SubjectNameNatCur { set; get; }
        public string IdSubjectType { set; get; }
        public string SubjectTypeName { set; get; }
        public string IdSubjectLevel { set; get; }
        public string? ReportType { set; get; }
        public int OrderNo { set; get; }
        public int Semester { set; get; }
    }
}
