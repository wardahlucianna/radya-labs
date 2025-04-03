using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.SubjectMapping
{
    public class GetSubjectMappingStudentScoreRequest
    {
        public string IdSubjectScoreSettingTarget { get; set; }
        public string IdComponentTarget { get; set; }
        public string IdSubComponentTarget { get; set; }
        public string IdStudent { get; set; }
    }
}
