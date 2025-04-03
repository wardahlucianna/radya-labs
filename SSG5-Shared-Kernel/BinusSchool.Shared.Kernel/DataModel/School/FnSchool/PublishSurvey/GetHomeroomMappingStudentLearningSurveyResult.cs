using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.PublishSurvey
{
    public class GetHomeroomMappingStudentLearningSurveyResult : ItemValueVm
    {
        public List<LevelMappingStudentLearning> Levels { get; set; }
    }

    public class LevelMappingStudentLearning : ItemValueVm
    {
        public List<GradeMappingStudentLearning> Grades { get; set; }
    }

    public class GradeMappingStudentLearning : ItemValueVm
    {
        public List<HomeroomMappingStudentLearning> Homerooms { get; set; }
    }

    public class HomeroomMappingStudentLearning : ItemValueVm
    {
        public int Semester { get; set; }
    }
}
