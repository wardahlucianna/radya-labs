using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.SubjectScoreDescription
{
    public class GetSubjectScoreDescriptionRequest : CollectionRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public string IdSubjectType { get; set; }
        public string IdSubject { get; set; }
        public string IdSubjectLevel { get; set; }
        public string IdPeriod { get; set; }
        public bool GetAllId { get; set; }
    }
}
