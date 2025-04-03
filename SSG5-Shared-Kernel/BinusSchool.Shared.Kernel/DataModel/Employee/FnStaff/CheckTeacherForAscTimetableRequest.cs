using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Employee.FnStaff
{
    public class CheckTeacherForAscTimetableRequest
    {
        public List<string> ShortName { get; set; }
        public string IdSchool { get; set; }
    }
}
