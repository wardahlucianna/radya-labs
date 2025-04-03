using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.StudentParentScoreView
{
    public class GetAllGradeResult
    {
        public string IdLevel { get; set; }
        public string LevelCode { get; set; }
        public string LevelDescription { get; set; }
        public List<GetAllGradeResult_Grades> Grades { get; set; }
    }

    public class GetAllGradeResult_Grades
    {
        public string IdGrade { get; set; }
        public string GradeCode { get; set; }
        public string GradeDescription { get; set; }
    }
}
