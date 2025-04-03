using BinusSchool.Common.Model;
//using BinusSchool.Common.Model.Information;

namespace BinusSchool.Data.Model.Student.FnStudent.SiblingGroup
{
    public class GetSiblingGroupDetailResult : ItemValueVm
    {
        public string IdStudent { get; set; }
        public AuditableResult Audit { get; set; }
    }
}