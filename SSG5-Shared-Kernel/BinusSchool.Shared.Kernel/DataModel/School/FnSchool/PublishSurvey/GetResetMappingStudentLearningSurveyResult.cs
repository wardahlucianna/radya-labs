using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.School.FnSchool.PublishSurvey
{
    public class GetResetMappingStudentLearningSurveyResult
    {
        public List<GetSurveyTeacher> Header { get; set; }
        public List<IDictionary<string, object>> MappingStudentLearningSurvey { get; set; }
    }

    public class GetSurveyTeacher
    {
        public string IdUser { get; set; }
        public string Code { get; set; }
        public string TeacherName { get; set; }
        public string ClassId { get; set; }
        public string IdHomeroom { get; set; }
        public string IdLesson { get; set; }
        public string Subject { get; set; }
        public bool IsReligion { get; set; }
    }
}
