using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.School.FnSchool.PublishSurvey
{
    public class GetResetMappingStudentLearningSurveyRequest : CollectionSchoolRequest
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdHomeroom { get; set; }
        public string IdPublishSurvey { get; set; }
        public string IdStudent { get; set; }
    }
}
