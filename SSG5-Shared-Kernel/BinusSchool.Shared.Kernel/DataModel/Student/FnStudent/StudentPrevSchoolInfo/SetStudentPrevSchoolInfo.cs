using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentPrevSchoolInfo
{
    public class SetStudentPrevSchoolInfo
    {
        public ItemValueVm IdPreviousSchoolNew { get; set; }
        public string Grade { get; set; }
        public string YearAttended { get; set; }
        public string YearWithdrawn { get; set; }
        //public Int16 IsHomeSchooling { get; set; }
    }
}
