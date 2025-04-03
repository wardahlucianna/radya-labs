using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.SubjectSplitMapping
{
    public class GetSubjectSplitMappingDetailRequest
    {
        public string IdAcademicYear {  get; set; }
        public string IdGrade { get; set; }
        public string IdParentSubject { get; set; }
       
    }
}
