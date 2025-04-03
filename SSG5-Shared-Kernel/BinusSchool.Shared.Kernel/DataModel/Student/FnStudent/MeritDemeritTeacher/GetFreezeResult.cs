using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.MeritDemerit
{
    public class GetFreezeResult : CodeWithIdVm
    {
        public string IdHomeroomStudent { get; set; }
        public string IdAcademicYear { get; set; }
        public string AcademicYear { get; set; }
        public string Level { get; set; }
        public string Grade { get; set; }
        public string IdGrade { get; set; }
        public string Homeroom { get; set; }
        public string IdStudent { get; set; }
        public string NameStudent { get; set; }
        public string Semester { get; set; }
        public bool IsFreeze { get; set; }


    }
}
