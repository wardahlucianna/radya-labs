namespace BinusSchool.Data.Model.Student.FnStudent.SiblingGroup
{
    public class AddSiblingGroupRequest
    {
        public string IdSiblingGroup { get; set; }
        public string IdStudent { get; set; }
        public string IdSibling { get; set; }
        public int IsParentUpdate { get; set; }
    }
}
