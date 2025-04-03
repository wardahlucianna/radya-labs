using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.SubjectMapping
{
    public class SaveSubjectMappingV2Request
    {
        public string IdSubjectMapping { get; set; }
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdGrade { get; set; }
        public string IdStreaming { get; set; }
    }
}
