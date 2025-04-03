using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentPrevSchoolInfo
{
    public class UpdateStudentPrevSchoolInfoRequest
    {
        public string IdStudent { get; set; }
        //public string IdPreviousSchoolOld { get; set; }
        public string IdPreviousSchoolNew { get; set; }
        public string IdPreviousSchoolNewDesc { get; set; }
        public string Grade { get; set; }
        public string YearAttended { get; set; }
        public string YearWithdrawn { get; set; }
        public Int16 IsHomeSchooling { get; set; }
        public int IsParentUpdate { get; set; }
    }
}
