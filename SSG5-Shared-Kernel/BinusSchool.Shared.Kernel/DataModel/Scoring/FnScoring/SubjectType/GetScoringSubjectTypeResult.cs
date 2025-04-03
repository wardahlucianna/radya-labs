using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.SubjectType
{
    public class GetScoringSubjectTypeResult
    {
        public NameValueVm Student { set; get; }
        public ItemValueVm AcademicYear { set; get; }
        public ItemValueVm Homeroom { set; get; }
        public ItemValueVm Grade { set; get; }
        public List<GetScoringSubjectTypeResult_SubjectType> SubjectType { set; get; }
    }
    public class GetScoringSubjectTypeResult_SubjectType
    {
        public string IdSubjectType { set; get; }
        public string SubjectTypeDesc { set; get; }
        //public string Type { set; get; }
        public string Code { get; set; }
    }
}
