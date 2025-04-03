using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.School.FnSchool.PublishSurvey
{
    public class MappingStudentLearningValueResult
    {
        public string IdLesson { get; set; }
        public string IdUserTeacher { get; set; }
        public bool IsChecked { get; set; }
        public bool IsReligion { get; set; }

    }
}
