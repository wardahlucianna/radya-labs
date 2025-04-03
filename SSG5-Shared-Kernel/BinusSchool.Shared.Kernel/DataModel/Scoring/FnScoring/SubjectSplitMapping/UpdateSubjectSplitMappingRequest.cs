using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.SubjectSplitMapping
{
    public class UpdateSubjectSplitMappingRequest
    {        
        public string IdAcademicYear { get; set; }
        public string IdGrade { get; set; }
        public string IdSubjectParent { get; set; }
        public List<string> SubjectChilds { get; set; }     
    }
}
