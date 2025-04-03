using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.SubjectMapping
{
    public class SaveSubjectMappingDetailV2Request
    {
        public string IdSubjectMapping { get; set; }
        public SaveSubjectMappingDetailRequestV2_Target? SubjectMappingTarget { get; set; }
        public List<SaveSubjectMappingDetailRequestV2_Source>? SubjectMappingSourceList { get; set; }

    }
    public class SaveSubjectMappingDetailRequestV2_Target
    {
        public string IdSubjectSemesterSetting { get; set; }
        public string IdSubjectScoreSetting { get; set; }
        public string IdComponent { get; set; }
        public string IdSubComponent { get; set; }
        public string IdCounter { get; set; }
    }

    public class SaveSubjectMappingDetailRequestV2_Source
    {
        public string IdSubjectSemesterSetting { get; set; }

        public string IdSubjectScoreSetting { get; set; }
        public string IdComponent { get; set; }
        public string IdSubComponent { get; set; }
        public decimal Weight { get; set; }
    }
}
