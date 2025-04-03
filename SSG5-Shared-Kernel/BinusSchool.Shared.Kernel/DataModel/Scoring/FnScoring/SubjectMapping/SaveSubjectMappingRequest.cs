using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.SubjectMapping
{
    public class SaveSubjectMappingRequest
    {
        public string IdSubjectMapping { get; set; }
        //public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string Term { get; set; }
        //public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        //public string IdPathway { get; set; }
        public string IdTargetCurriculum { get; set; }
        public string IdTargetSubjectType { get; set; }
    }
}
