using System;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentExitForm
{
    public class GetStudentExitFormResult : CodeWithIdVm
    {
        public string IdSchool { get; set; }
        public string SchoolName { get; set; }
        public ItemValueVm AcademicYear { get; set; }
        public StatusExitStudent Status { get; set; }
        public string IdHomeroomStudent { get; set; }
        public string IdStudent { get; set; }
        public string StudentName { get; set; }
        public DateTime SubmissionDate { get; set; }
        public DateTime LastDateOfAttendance { get; set; }
        public string ApproveNote { get; set; }
    }
}
