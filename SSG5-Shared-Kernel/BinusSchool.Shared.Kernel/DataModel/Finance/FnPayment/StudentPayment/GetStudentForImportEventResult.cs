using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Finance.FnPayment.StudentPayment
{
    public class GetStudentForImportEventResult
    {
        public string IdStudent { get; set; }
        public string StudentName { get; set; }
        public string SchoolLevel { get; set; }
        public string YearLevel { get; set; }
        public string IdHomeroomStudent { get; set; }
        public string Homeroom { get; set; }
    }
}
