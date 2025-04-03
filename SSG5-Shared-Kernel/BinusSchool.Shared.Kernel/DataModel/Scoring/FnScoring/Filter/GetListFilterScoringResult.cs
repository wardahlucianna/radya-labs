using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.Filter
{
    public class GetListFilterScoringResult
    {
        public List<GetListFilterScoringResult_Level> Level { get; set; }
        public List<GetListFilterScoringResult_Grade> Grade { get; set; }
        public List<GetListFilterScoringResult_Semester> Semester { get; set; }
        public List<GetListFilterScoringResult_Term> Term { get; set; }
        public List<GetListFilterScoringResult_Subject> Subject { get; set; }
        public List<GetListFilterScoringResult_SubjectType> SubjectType { get; set; }
        public List<GetListFilterScoringResult_SubjectWithLevel> SubjectWithLevel { get; set; }
        public List<GetListFilterScoringResult_SubjectChild> SubjectChild { get; set; }
        public List<GetListFilterScoringResult_Homeroom> Homeroom { get; set; }

        public List<GetListFilterScoringResult_Streaming> Streaming { get; set; }
    }

    public class GetListFilterScoringResult_Level : CodeWithIdVm
    {
        public int OrderNumber { get; set; }
    }

    public class GetListFilterScoringResult_Grade : CodeWithIdVm
    {
        public string IdLevel { get; set; }
        public int OrderNumber { get; set; }
    }

    public class GetListFilterScoringResult_SubjectType : CodeWithIdVm
    {
        public string IdGrade { get; set; }
    }

    public class GetListFilterScoringResult_Subject : CodeWithIdVm
    {
        public string IdSubjectType { get; set; }
        public string IdGrade { get; set; }
    }

    public class GetListFilterScoringResult_SubjectWithLevel : CodeWithIdVm
    {
        public string IdGrade { get; set; }
        public string IdSubjectType { get; set; }
        public string IdSubject { get; set; }
        public string SubjectName { get; set; }
        public string IdSubjectLevel { get; set; }
        public string SubjectLevelName { get; set; }
    }

    public class GetListFilterScoringResult_SubjectChild : CodeWithIdVm
    {
        public string IdGrade { get; set; }
        public string IdSubject { get; set; }
    }

    public class GetListFilterScoringResult_Semester : CodeWithIdVm
    {
        public string IdGrade { get; set; }
        public int OrderNumber { get; set; }
    }

    public class GetListFilterScoringResult_Term : CodeWithIdVm
    {
        public string IdGrade { get; set; }
        public int Semester { get; set; }
        public int OrderNumber { get; set; }
    }

    public class GetListFilterScoringResult_Homeroom : CodeWithIdVm
    {
        public string IdGrade { get; set; }
        public int Semester { get; set; }
    }

    public class GetListFilterScoringResult_Streaming : CodeWithIdVm
    {
        public string IdGrade { get; set; }

    }
}
