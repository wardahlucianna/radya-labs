using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Information;

namespace BinusSchool.Data.Model.Student.FnStudent.Student
{
    public class GetStudentBySiblingGroupResult : ItemValueVm
    {

        public NameInfoVm nameInfo { get; set; }
        public string IdSiblingGroup { get; set; }
        public string Homeroom { get; set; }
        public string IdStudentEncryptedRjindael { get; set; }
        public string IdStudentEncrypted {  get; set; }

    }
}
