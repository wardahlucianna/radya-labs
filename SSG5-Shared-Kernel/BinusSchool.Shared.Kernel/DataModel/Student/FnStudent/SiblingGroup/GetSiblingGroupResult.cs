using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.SiblingGroup
{
    public class GetSiblingGroupResult : ItemValueVm
    {
        public string IdStudent { get; set; }
        public AuditableResult Audit { get; set; }
    }
}