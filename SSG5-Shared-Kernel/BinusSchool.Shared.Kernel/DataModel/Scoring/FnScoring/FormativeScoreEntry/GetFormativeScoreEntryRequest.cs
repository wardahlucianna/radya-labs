using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.FormativeScoreEntry
{
    public class GetFormativeScoreEntryRequest
    {
        public string IdSchool { set; get; }
        public int Semester { set; get; }
        public string IdPeriod { set; get; }
        public string IdGrade { set; get; }
        public string IdSubject { set; get; }
        public string IdLesson { set; get; }
        public string IdSubjectLevel { set; get; }
        public string Component { get; set; }
        public string SubComponent { get; set; }
        public string SubComponentCounter { get; set; }
    }
}
