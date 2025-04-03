using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ProgressStatus.Filter
{
    public class GetSpecificStudentsByPositionRequest : CollectionRequest
    {
        public string PositionShortName { get; set; }
        public string IdUser { get; set; }
        public string IdSchool { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdGrade { get; set; }
        public string IdHomeroom { get; set; }
        public int Semester { get; set; }
    }
}
