using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.School.FnSchool.MySurvey
{
    public class GetMySurveyRequest : CollectionSchoolRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdUser { get; set; }
        public int Semester { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string IdUserParent { get; set; }
    }
}
