using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentExitForm
{
    public class GetStudentExitFormDetailResult
    {
        public string Id { get; set; }
        public ItemValueVm AcademicYear { get; set; }
        public string IdHomeroomStudent { get; set; }
        public string IdStudent { get; set; }
        public string StudentName { get; set; }
        public string IdUserFather { get; set; }
        public string UserFatherName { get; set; }
        public string FatherEmail { get; set; }
        public string FatherPhone { get; set; }
        public string IdUserMother { get; set; }
        public string UserMotherName { get; set; }
        public string MotherEmail { get; set; }
        public string MotherPhone { get; set; }
        public DateTime StartExit { get; set; }
        public List<ReasonExitStudent> ReasonExitStudents { get; set; }
        public string Explain { get; set; }
        public bool IsMeetSchoolTeams { get; set; }
        public string NewSchoolName { get; set; }
        public string NewSchoolCity { get; set; }
        public string NewSchoolCountry { get; set; }
        public StatusExitStudent Status { get; set; }
        public ExitStudentStatus ExitStudentStatuses { get; set; }
        public ItemValueVm Homeroom { get; set; }
        public ItemValueVm Level { get; set; }
        public ItemValueVm Grade { get; set; }
        public string IdUserin { get; set; }
        public bool IsParent { get; set; }
    }

    public class ReasonExitStudent
    {
        public string Id { get; set; }
        public string Reason { get; set; }
    }

    public class ExitStudentStatus
    {
        public string Id { get; set; }
        public string Status { get; set; }
        public string Note { get; set; }
    }
}
