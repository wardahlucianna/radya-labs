using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.SubjectMapping
{
    public class GetSubjectMappingStudentScoreResult
    {
        public string IdSubjectMapping { get; set; }
        public string IdSubjectMappingDetail { get; set; }
        public GetSubjectMappingStudentScoreResult_Target SubjectMappingTarget { get; set; }
        public GetSubjectMappingStudentScoreResult_Source SubjectMappingSource { get; set; }
    }
    public class GetSubjectMappingStudentScoreResult_Target
    {
        public GetSubjectMappingStudentScoreResult_SubjectVm Subject { get; set; }
        public ItemValueVm Component { get; set; }
        public ItemValueVm SubComponent { get; set; }
    }

    public class GetSubjectMappingStudentScoreResult_Source
    {
        public GetSubjectMappingStudentScoreResult_SubjectVm Subject { get; set; }
        public ItemValueVm Component { get; set; }
        public ItemValueVm SubComponent { get; set; }
        public decimal Weight { get; set; }
        public decimal Score { get; set; }
    }

    public class GetSubjectMappingStudentScoreResult_SubjectVm
    {
        public string IdSubjectScoreSetting { get; set; }
        public string Description { get; set; }
    }
}
