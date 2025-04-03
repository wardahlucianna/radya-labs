using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ReportSetting.ScoreViewMapping
{
    public class GetSoreViewMappingByViewTemplateResult
    {        
        public List<GetScoreViewMapping_SubjectScoreMappingVm> SubjectMappingList { set; get; }
        public bool CurrentStatus { set; get; }
    }

    public class GetScoreViewMapping_SubjectScoreMappingVm
    {
        public ItemValueVm Curriculum { set; get; }
        public List<GetSoreViewMapping_SubjectTypesVm> SubjectTypes { set; get; }
    }

    public class GetSoreViewMapping_SubjectTypesVm
    {
        public ItemValueVm SubjectType { set; get; }
        public List<GetSoreViewMapping_SubjectsVm> Subjects { set; get; }
    }
    public class GetSoreViewMapping_SubjectsVm
    {
        public CodeWithIdVm Subject { set; get; }
        public bool SubjectTaken { set; get; }
        public int? OrderNo { set; get; }
    }
}
