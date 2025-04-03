using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.SubjectMapping
{
    public class GetSubjectMappingDetailV2Result
    {
        public string IdSubjectMappingDetail { get; set; }
        public GetSubjectMappingDetailV2Result_Target SubjectMappingTarget { get; set; }
        public GetSubjectMappingDetailV2Result_Source SubjectMappingSource { get; set; }
    }
    public class GetSubjectMappingDetailV2Result_Target
    {
        public GetSubjectMappingDetailV2Result_SubjectSemesterVm SubjectScoreSemester { get; set; }
        public ItemValueVm SubjectScore { get; set; }
        public ItemValueVm Component { get; set; }
        public ItemValueVm SubComponent { get; set; }
        public ItemValueVm SubComponentCounter { get; set; }
    }

    public class GetSubjectMappingDetailV2Result_Source
    {
        public GetSubjectMappingDetailV2Result_SubjectSemesterVm SubjectScoreSemester { get; set; }
        public ItemValueVm SubjectScore { get; set; }
        public ItemValueVm Component { get; set; }
        public ItemValueVm SubComponent { get; set; }
        public decimal Weight { get; set; }
    }

    public class GetSubjectMappingDetailV2Result_SubjectSemesterVm
    {
        public string IdSubjectScoreSemesterSetting { get; set; }
        public string Description { get; set; }
    }
}
