using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher
{
    public class GetDetailDemeritTeacherResult
    {
        public List<DetailMeritDemeritTeacher> Demerit { get; set; }
        public bool IsShowDemerit { get; set; } 
    }
}
