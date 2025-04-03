using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Teaching.FnAssignment.SubjectCombination
{
    public class GetSubjectCombinationResult
    {
        public CodeWithIdVm Grade { get; set; }
        public List<SubjectData> SubjectDataRow { get; set; } = new List<SubjectData>();
        public List<ClassData> ClassDataColumns { get; set; } = new List<ClassData>();
        public List<SubjectDataVM> SubjectDataVM { get; set; } = new List<SubjectDataVM>();
        public AuditableResult Audit { get; set; }
    }

    public class SubjectData: CodeVm
    {
        public string Id { get; set; }
        public int TotalSession { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public List<PathwayData> Pathways { get; set; } = new List<PathwayData>();
        public List<SubjectCombinationData> SubjectCombinationData { get; set; } = new List<SubjectCombinationData>();
    }

    public class SubjectCombinationData 
    {
        public string Id { get; set; }
        public string IdClassGrade { get; set; }
        public string IdSubject { get; set; }
        public bool IsDelete { get; set; }
        public bool IsAlreadyUse { get; set; }
    }

    public class ClassData : CodeVm
    {
        public string Id { get; set; }
        public List<PathwayData> Pathways { get; set; }
    }

    public class PathwayData : CodeVm
    {
        public string Id { get; set; }
    }

    public class SubjectDataVM : CodeVm
    {
        public string Id { get; set; }
        public int TotalSession { get; set; }
        public string Grade {get;set;}
        public List<PathwayData> Pathway { get; set; } = new List<PathwayData>();
        public List<SubjectCombinationData> SubjectCombinationData { get; set; } = new List<SubjectCombinationData>();
        public List<ClassDataVM> ClassData { get; set; } = new List<ClassDataVM>();
    }

    public class ClassDataVM
    {
        public string Id { get; set; }
        public string IdClassGrade { get; set; }
        public string IdSubject { get; set; }
        public bool IsDelete { get; set; }
        public bool IsAlreadyUse { get; set; }
        public bool IsCheck { get; set; }
        public List<PathwayData> Pathways { get; set; }
    }
}
