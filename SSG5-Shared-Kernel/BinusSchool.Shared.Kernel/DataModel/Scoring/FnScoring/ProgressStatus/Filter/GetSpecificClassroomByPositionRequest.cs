using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ProgressStatus
{
    public class GetSpecificClassroomByPositionRequest : CollectionRequest
    {
        public string IdSchool { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdUser { get; set; }
        public string IdGrade { get; set; }
        public int Semester { get; set; }
        public string PositionShortName { get; set; }
    }
}
