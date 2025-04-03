using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.PublishSurvey
{
    public class GetResetMappingStudentLearning
    {
        public List<GetSurveyTeacher> Teacher { get; set; }
        public List<GetStudentLearning> Student { get; set; }
    }

    public class GetStudentLearning
    {
        public string IdStudent { get; set; }
        public string StudentName { get; set; }
        public string IdHomeroom { get; set; }
        public string IdHomeroomStudent { get; set; }
        public string Religion { get; set; }
        public List<GetMappingStudent> Mapping { get; set; }
    }
    public class GetMappingStudent : ItemValueVm
    {
        public string IdLesson { get; set; }
        public string IdUserTeacher { get; set; }
        public bool IsChecked { get; set; }
        public bool IsReligion { get; set; }

    }
}
