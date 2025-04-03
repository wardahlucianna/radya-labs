using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.School.FnSchool.PublishSurvey
{
    public class GetPublishSurveyRequest : CollectionSchoolRequest
    {
        public string IdAcademicYear { get; set; }
        public int? Semester { get; set; }
        public PublishSurveyType? SurveyType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
    }
}
