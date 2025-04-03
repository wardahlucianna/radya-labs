using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentPrevSchoolInfo
{
    public class GetStudentPrevSchoolInfoData
    {
        public string Grade { get; set; }
        public string YearAttended { get; set; }
        public string YearWithdrawn { get; set; }
        public Int16 IsHomeSchooling { get; set; }
        public string IdPreviousSchoolNew { get; set; }
        public string IdPreviousSchoolOld { get; set; }
    }
}
