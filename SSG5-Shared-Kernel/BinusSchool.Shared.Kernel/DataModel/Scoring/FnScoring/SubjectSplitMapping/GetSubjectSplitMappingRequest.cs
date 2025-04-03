using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.SubjectSplitMapping
{
    public class GetSubjectSplitMappingRequest : CollectionRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }       
        public string IdParentSubject { get; set; }
        public string IdChildSubject { get; set; }

    }
}
