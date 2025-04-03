using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentInfoUpdate
{
    public class GetStudentInfoUpdateRequest : CollectionRequest
    {
        public string IdUser { get; set; }
        public string TableName { get; set; }
        public string FieldName { get; set; }
        public string OldFieldValue { get; set; }
        public string CurrentFieldValue { get; set; }
        public int IdApprovalStatus { get; set; }
        public int IsParentUpdate { get; set; }
        public string Notes { get; set; }

    }
}
