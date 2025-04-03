using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.School.FnSchool.PublishSurvey
{
    public class AddMappingStudentLearningSurveyRequest
    {
        public string IdPublishSurvey { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdHomeroom { get; set; }
        public List<MappingStudentLearningSurvey> MappingStudentLearningSurveys { get; set; }
    }

    public class MappingStudentLearningSurvey
    {
        public string BinusianId { get; set; }
        public string IdHomeroomStudent { get; set; }
        public string IdLesson { get; set; }
    }
}
