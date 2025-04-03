using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.P5Projects.P5Entry
{
    public class GetStudentP5EntryResult
    {
        public List<GetStudentP5Score_DimensionVm> Dimensions { get; set; }
        public List<GetStudentP5Score_StudentCommentVm> StudentsComment { get; set; }
        public List<GetStudentP5Score_PredicatesVm> Predicates { get; set; }
        public GetStudentP5Score_ProjectVm Project { get; set; }
    }

    public class GetStudentP5Score_DimensionVm
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public List<GetStudentP5Score_OutcomesVm> Outcomes { get; set; }
    }
    public class GetStudentP5Score_OutcomesVm
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public List<GetStudentP5Score_ScoreVm> Score { get; set; }
        public GetStudentP5Score_LastMofiedVm scoreLastModifiedBy { get; set; }
    }
    public class GetStudentP5Score_ScoreVm
    {
        public string IdP5Score { get; set; }
        public string IdStudent { get; set; }
        public string StudentName { get; set; }
        public int? Score { get; set; }
    }
    public class GetStudentP5Score_LastMofiedVm
    {
        public DateTime Date { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class GetStudentP5Score_StudentCommentVm
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Comment { get; set; }
    }
    public class GetStudentP5Score_PredicatesVm
    {
        public string Id { get; set; }
        public int Key { get; set; }
        public string Code { get; set; }
        public string ShortDesc { get; set; }
        public string Description { get; set; }
    }

    public class GetStudentP5Score_ProjectVm
    {
        public string Topic { get; set; }
        public string Description { get; set; }
    }
}
