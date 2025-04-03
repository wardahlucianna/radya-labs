using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.SubjectScoreDescription
{
    public class SaveSubjectScoreDescriptionRequest
    {
        public List<string> IdStudentSubjectScoreSettingCombined { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
