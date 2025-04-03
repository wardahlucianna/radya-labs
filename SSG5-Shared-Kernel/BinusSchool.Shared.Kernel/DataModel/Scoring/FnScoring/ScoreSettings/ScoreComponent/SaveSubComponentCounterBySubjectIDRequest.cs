using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.ScoreComponent
{
    public class SaveSubComponentCounterBySubjectIDRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdGrade { get; set; }
        public string IdSubject { get; set; }
        public string IdSubComponent { get; set; }
        public string IdSubjectToCreateCounter { get; set; }
        public string ShortDesc { get; set; }
        public string Description { get; set; }
        public bool TeacherCanEditMaxRawScore { get; set; }
        public List<string> ClassIds { get; set; }
    }
}
