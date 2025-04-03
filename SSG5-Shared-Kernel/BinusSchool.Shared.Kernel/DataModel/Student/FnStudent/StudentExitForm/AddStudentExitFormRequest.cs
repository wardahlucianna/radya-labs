using System;
using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentExitForm
{
    public class AddStudentExitFormRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdHomeroomStudent { get; set; }
        public string IdUserFather { get; set; }
        public string FatherEmail { get; set; }
        public string FatherPhone { get; set; }
        public string IdUserMother { get; set; }
        public string MotherEmail { get; set; }
        public string MotherPhone { get; set; }
        public DateTime StartExit { get; set; }
        public List<string> ReasonExitStudent { get; set; }
        public string Explain { get; set; }
        public bool IsMeetSchoolTeams { get; set; }
        public string NewSchoolName { get; set; }
        public string NewSchoolCity { get; set; }
        public string NewSchoolCountry { get; set; }
        public StatusExitStudent Status { get; set; }
        public string IdHomeroom { get; set; }
        public bool IsParent { get; set; }
    }
}
