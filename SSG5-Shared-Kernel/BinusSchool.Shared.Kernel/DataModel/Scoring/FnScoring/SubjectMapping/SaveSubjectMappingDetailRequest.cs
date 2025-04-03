using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.SubjectMapping
{
    public class SaveSubjectMappingDetailRequest
    {
        public string IdSubjectMapping { get; set; }
        public SaveSubjectMappingDetailRequest_Target SubjectMappingTarget { get; set; }
        public List<SaveSubjectMappingDetailRequest_Source> SubjectMappingSourceList { get; set; }

    }
    public class SaveSubjectMappingDetailRequest_Target
    {
        public string IdSubjectScoreSetting { get; set; }
        public string IdComponent { get; set; }
        public string IdSubComponent { get; set; }
    }

    public class SaveSubjectMappingDetailRequest_Source
    {
        public string IdSubjectScoreSetting { get; set; }
        public string IdComponent { get; set; }
        public string IdSubComponent { get; set; }
        public decimal Weight { get; set; }
    }
}
