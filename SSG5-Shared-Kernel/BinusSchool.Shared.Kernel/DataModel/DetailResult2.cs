using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.School;

namespace BinusSchool.Data.Model
{
    public class DetailResult2<T> : CodeWithIdVm<T>
    {
        public GetSchoolResult School { get; set; }
        public AuditableResult Audit { get; set; }
        public bool CanEditName { get; set; }
    }

    public class DetailResult2 : DetailResult2<string> {}
}
