using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentExitForm
{
    public class GetParentByChildRequest
    {
        public string IdStudent { get; set; }
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
    }
}
