using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher
{
    public class GetEntryMeritDemeritStudentByFreezeResult 
    {
        public string IdHomeroomStudent { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string IdBinusan { get; set; }
        public string Grade { get; set; }
        public CodeWithIdVm Homeroom { get; set; }
    }
}
