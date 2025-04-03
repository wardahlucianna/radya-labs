namespace BinusSchool.Data.Model.Student.FnStudent.StudentInfoUpdate
{
    public class GetStudentInfoUpdateResult
    {
        public string IdUser { get; set; }
        public string TableName { get; set; }
        public string FieldName { get; set; }
        public string OldFieldValue { get; set; }
        public string CurrentFieldValue { get; set; }
        public string Constraint1 { get; set; }
        public string Constraint1Value { get; set; }
        public string Constraint2 { get; set; }
        public string Constraint2Value { get; set; }
        public string Constraint3 { get; set; }
        public string Constraint3Value { get; set; }
        public int IsParentUpdate { get; set; }
        public string RequestedBy { get; set; }
    }
}
