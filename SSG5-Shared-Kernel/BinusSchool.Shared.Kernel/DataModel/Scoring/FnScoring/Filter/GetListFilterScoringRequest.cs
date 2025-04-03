using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.Filter
{
    public class GetListFilterScoringRequest
    {
        public string IdUser { get; set; }
        public string IdSchool { get; set; }
        public string IdAcademicYear { get; set; }
        public bool ShowLevel { get; set; }
        public bool ShowGrade { get; set; }
        public bool ShowSemester { get; set; }
        public bool ShowTerm { get; set; }
        public bool ShowHomeroom { get; set; }
        public bool ShowSubject { get; set; }
        public bool ShowSubjectChild { get; set; }
        public bool ShowSubjectWithLevel { get; set; }
        public bool ShowSubjectType { get; set; }
        //public bool ShowLesson { get; set; }
        public bool ShowStudent { get; set; }
        public bool ShowStreaming { get; set; }
    }
}
