using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.EmergencyAttendance
{
    public class UnsubmittedStudentResult
    {
        public ItemValueVm Student { get; set; }
        public CodeWithIdVm Level { get; set; }
        public ItemValueVm Homeroom { get; set; }
    }
}
